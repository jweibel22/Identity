using System;
using Identity.Domain;

namespace Identity.Rest
{
    class AutoMapperConfiguration
    {
        public static void Configure()
        {
            AutoMapper.Mapper.CreateMap<Channel, Infrastructure.DTO.Channel>()
                .ForMember(x => x.IsPrivate, _ => _.MapFrom(src => !src.IsPublic));

            AutoMapper.Mapper.CreateMap<Post, Infrastructure.DTO.Post>()
                .ForMember(x => x.Type, _ => _.UseValue("link"))
                .ForMember(x => x.IsCollapsed, _ => _.UseValue(true))
                .ForMember(x => x.Teaser, _ => _.ResolveUsing(MapTeaser));

            AutoMapper.Mapper.CreateMap<Comment, Infrastructure.DTO.Comment>()
                .ForMember(x => x.Body, _ => _.MapFrom(src => src.Text));

            AutoMapper.Mapper.CreateMap<RssFeeder, Infrastructure.DTO.RssFeeder>();

            AutoMapper.Mapper.CreateMap<WeightedTag, Infrastructure.DTO.WeightedTag>()
                .ForMember(x => x.weight, _ => _.MapFrom(src => src.Weight))
                .ForMember(x => x.text, _ => _.MapFrom(src => src.Text));

        }

        private static string MapTeaser(Post post)
        {
            if (post.Description.Length < 500)
            {
                return null;
            }

            var result = post.Description.Substring(0, Math.Min(post.Description.Length, 500));

            var lastEnd = result.LastIndexOf('>');
            var lastBegin = result.LastIndexOf('<');

            if (lastBegin > lastEnd)
            {
                result = result.Substring(0, lastBegin);
            }

            return result;
        }
    }
}