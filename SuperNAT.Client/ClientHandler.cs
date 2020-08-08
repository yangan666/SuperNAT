using SuperNAT.Core;

namespace SuperNAT.Client
{
    public class ClientHandler
    {
        public void Start()
        {
            ClientManager.Start();
        }

        public void Stop()
        {
            ClientManager.Stop();
        }
    }
}
