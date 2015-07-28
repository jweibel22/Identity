using Topshelf;

namespace Identity.Fetcher.WindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ServiceLifecycle>(s =>
                {
                    s.ConstructUsing(name => new ServiceLifecycle());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();
            });
        }
    }
}
