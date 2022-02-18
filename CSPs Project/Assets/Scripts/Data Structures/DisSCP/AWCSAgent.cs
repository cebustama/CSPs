using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

public class AWCSAgent<T> : DiSCPAgent<T>
{
    private AWCSManager<T> awcsManager;

    private List<List<DiSCPAgentViewTuple<T>>> sentNoGoods = new List<List<DiSCPAgentViewTuple<T>>>();
    private List<DiSCPAgentViewTuple<T>> sentNoGoodElements = new List<DiSCPAgentViewTuple<T>>();

    public AWCSAgent(AWCSManager<T> manager, string iD) : base(manager, iD, 0)
    {
        awcsManager = manager;
    }

    // AWCS sends ok message to all neighbors
    public void SendOK()
    {
        UnityEngine.Debug.Log("<color=green>" + Name + " sending OK to all neighbors." + "</color>");

        awcsManager.SendMessage(
            new AWCSMessage<T>(
                DiSCPAgentMessage<T>.MessageType.OK,
                new List<DiSCPAgentViewTuple<T>>() { new DiSCPAgentViewTuple<T>(Name, value, (uint)Priority) },
                Name
            ),
            this, Neighbors.ConvertAll(nId => manager.CSP.VariableNames[nId])
        );
    }

    public bool SendNoGood()
    {
        // COPY Current VIEW
        List<DiSCPAgentViewTuple<T>> noGood =
            //awcsManager.GetLowerAgents(this, new List<List<DiSCPAgentViewTuple<T>>>() { View })[0];
            View;

        // NO SOLUTION
        if (noGood.Count == 0) return false;

        // Check that no element of nogoods has been sent before
        /*foreach (var ng in noGood)
        {
            if (sentNoGoodElements.Contains(ng)) return false;
        }*/

        // If this no good hasn't been sent
        if (!sentNoGoods.Contains(noGood))
        {
            foreach (var tuple in noGood)
            {
                sentNoGoodElements.Add(tuple);
                awcsManager.SendMessage(
                    new AWCSMessage<T>(DiSCPAgentMessage<T>.MessageType.NOGOOD, noGood, Name),
                    this,
                    new List<string>() { tuple.Name }
                );
            }
            sentNoGoods.Add(noGood);

            // Set priority as max between elements in view + 1
            uint maxPriority = 0;
            foreach (var n in View)
            {
                maxPriority = math.max(maxPriority, n.priority);
            }
            SetPriority((int)maxPriority + 1);

            // Asignar valor que minimice restricciones violadas con agentes de menor prioridad (todos en este momento)
            // TODO: FIX THIS, esta vez no debería checkear si son consistentes o no
            // REVISAR CONDICIONES DE TERMINO
            AssignValue(false);

            UnityEngine.Debug.Log("<color=cyan>" + Name + " assigned value " + value + "</color>");

            // Enviar OK a vecinos
            SendOK();

            return true;
        }

        return false;
    }

    public override bool AssignValue(bool checkConsistency = true)
    {
        return manager.AssignValue(Name, checkConsistency);
    }
}

public class AWCSMessage<T> : DiSCPAgentMessage<T>
{
    public AWCSMessage(MessageType t, List<DiSCPAgentViewTuple<T>> tuples, string sender)
    {
        senderID = sender;
        Type = t;

        Contents = new List<DiSCPAgentViewTuple<T>>();
        foreach (DiSCPAgentViewTuple<T> p in tuples)
        {
            Contents.Add(p);
        }
    }
}
