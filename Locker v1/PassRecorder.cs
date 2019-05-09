using System;
using System.Collections.Generic;

namespace Locker_v1
{
    public class PassRecorder
    {
        private Queue<char> queue;
        private string password;
        public event EventHandler OnValidPassword;
        public delegate void OnValidPasswordDelegate(object message, EventArgs e);
        private OnValidPasswordDelegate passwordDelegate;

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value.ToUpper();
            }
        }

        public PassRecorder()
        {
            queue = new Queue<char>();
        }

        public void Add(char c)
        {
            if(!string.IsNullOrWhiteSpace(password))
            {
                queue.Enqueue(c);

                if (queue.Count > Password.Length)
                {
                    queue.Dequeue();
                }

                string str = new string(queue.ToArray());

                if (str == Password)
                {
                    OnValidPassword?.Invoke(passwordDelegate, EventArgs.Empty);
                }
            }
        }
    }
}
