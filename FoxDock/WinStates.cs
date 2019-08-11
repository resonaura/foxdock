using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxDock
{
    public class WinStates
    {
        public List<string> names = new List<string>();
        public List<int> states = new List<int>();

        public void Set(string name, int state)
        {
            if (!names.Contains(name))
            {
                names.Add(name);
                states.Add(state);

            }
            else
            {
                states[names.IndexOf(name)] = state;
            }
        }
        public int Get(string name)
        {
            int index = names.IndexOf(name);
            if (index != -1)
            {
                return states[index];
            }
            else
            {
                return 1;
            }
        }
    }
}
