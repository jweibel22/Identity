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
        private readonly ChannelRepository channelRepository;

        public TagCloudRefresher(ChannelRepository channelRepository)
        {
            this.channelRepository = channelRepository;
        }

        public void Execute()
        {
            //TODO: this will not scale :-)
            var allChannels = channelRepository.All();
            
            foreach (var channel in allChannels)
            {
                var tagCloud = channelRepository.CalculateTagCloud(channel.Id);
                channelRepository.UpdateTagCloud(channel.Id, tagCloud);
            }
        }
    }
}
