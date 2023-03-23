using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class Subscriptions : IModel
    {
        public IList<Subscription> Subscribers { get; set; }

        public Subscriptions()
        {
            Subscribers = new List<Subscription>();
        }
    }
}
