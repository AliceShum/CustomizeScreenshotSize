using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    public delegate void event_handler(string event_name = null, object udata = null);

    private Dictionary<string, event_handler> eventDic = new Dictionary<string, event_handler>();

    public void AddListener(string event_name, event_handler h)
    {
        if (this.eventDic.ContainsKey(event_name))
        {
            this.eventDic[event_name] += h;
        }
        else
        {
            this.eventDic.Add(event_name, h);
        }
    }

    public void RemoveListener(string event_name, event_handler h)
    {
        if (!this.eventDic.ContainsKey(event_name))
        {
            return;
        }

        this.eventDic[event_name] -= h;

        if (this.eventDic[event_name] == null)
        {
            this.eventDic.Remove(event_name);
        }
    }

    public void DispatchEvent(string event_name, object udata)
    {
        //Debug.Log("Dispatch event：" + event_name);
        if (!this.eventDic.ContainsKey(event_name))
        {
            return;
        }

        this.eventDic[event_name](event_name, udata);
    }

}
