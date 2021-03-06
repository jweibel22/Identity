﻿create function [dbo].[ftFetchUnreadPosts] (@ChannelId bigint, @UserId bigint, @OrderByColumn int, @IsPremium int)
returns table
as return with cte as 
(
    select @ChannelId as Id
    union all
    select t.ChildId as Id from cte 
        inner join [ChannelLink] t on cte.Id = t.Parentid
)

select AllPosts.*, 
ROW_NUMBER() OVER (ORDER BY CASE @OrderByColumn
    WHEN 1 THEN DATEDIFF(second,{d '1970-01-01'},AllPosts.Added)
    WHEN 2 THEN AllPosts.Popularity
    END desc) AS RowNum from (

select Count(ci.PostId) as Popularity, ci.PostId,  
CASE WHEN Min(pc.LatestAdded) IS NULL THEN Min(ci.Created) ELSE Min(pc.LatestAdded) END AS Added,
Min(pcm.ClusterId) as PostClusterId
from ChannelItem ci
inner join cte on ci.ChannelId = cte.Id
left join PostClusterMember pcm on pcm.OntologyId = 1 and pcm.PostId = ci.PostId
left join PostCluster pc on pc.OntologyId = 1 and pc.ClusterId = pcm.ClusterId
inner join Post p on p.Id = ci.PostId
where (@IsPremium = 2 OR p.PremiumContent = @IsPremium) AND NOT EXISTS (SELECT PostId from ReadHistory where UserId = @UserId and PostId = ci.PostId)
group by ci.PostId) as AllPosts
