using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{

    interface ITask 
    {
        void Excute();

    }

    class BroadcastTask : ITask
    {

        GameRoom _room;
        ClientSession _session;
        string _chat;
        BroadcastTask(GameRoom room ,ClientSession session , string chat) 
        {
            _room = room;
            _session = session;
            _chat = chat;
        }
        public void Excute() 
        {
            _room.Broadcast(_session, _chat);
        }

    
    }
    class TaskQueue
    {
        Queue<ITask> taskQueue = new Queue<ITask>();
    }
}
