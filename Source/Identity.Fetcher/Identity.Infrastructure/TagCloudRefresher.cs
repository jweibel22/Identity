using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Infrastructure.Repositories;

namespace Identity.Infrastructure
{
    public class TagCloudRefresher
    {
        private readonly ConnectionFactory connectionFactory;

        public TagCloudRefresher(ConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public void Execute()
        {
            using (var session = connectionFactory.NewTransaction())
            {
                var channelRepo = new ChannelRepository(session.Transaction);

                //TODO: this will not scale :-)
                var allChannels = channelRepo.All();

                foreach (var channel in allChannels)
                {
                    var tagCloud = channelRepo.CalculateTagCloud(channel.Id).ToList();
                    channelRepo.UpdateTagCloud(channel.Id, tagCloud);
                }

                session.Commit();
            }
        }
    }
}
