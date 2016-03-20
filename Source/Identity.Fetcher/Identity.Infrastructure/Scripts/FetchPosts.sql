create procedure FetchPosts
	@UserId bigint,
	@FromIndex int,
	@ChannelId bigint,
	@Timestamp Date,
	@PageSize int
as 
begin
select * from (
select Post.*,
XX.pop as UserSpecificPopularity,
XX.Added as Added,
CASE WHEN liked.Created IS NULL THEN 'false' ELSE 'true' END as Liked, 
CASE WHEN saved.Created IS NULL THEN 'false' ELSE 'true' END as Saved, 
CASE WHEN starred.Created IS NULL THEN 'false' ELSE 'true' END as Starred, 
CASE WHEN ReadHistory.Timestamp IS NULL THEN 'false' ELSE 'true' END as [Read],
CASE WHEN pop.Popularity IS NULL THEN 0 ELSE pop.Popularity END as Popularity,
ROW_NUMBER() OVER (ORDER BY XX.Added desc) AS RowNum
from Post 
join 
(select ci.PostId, count(*) as pop, min(ci.Created) as Added
from Post 
join ChannelItem ci on ci.PostId = Post.Id 
join [User] u on u.Id = @UserId
left join ChannelLink cl on cl.ChildId = ci.ChannelId and cl.ParentId = @ChannelId
left join ChannelOwner co on co.ChannelId = ci.ChannelId and co.UserId = @UserId
join Channel c on c.Id = ci.ChannelId
where (ci.ChannelId=@ChannelId or cl.ParentId=@ChannelId) and (co.ChannelId is not null or c.IsPublic = 1)
group by ci.PostId) as XX on XX.PostId = Post.Id
join [User] u on u.Id = @UserId
left join ChannelItem liked on liked.ChannelId = u.LikedChannel and liked.PostId = Post.Id
left join ChannelItem saved on saved.ChannelId = u.SavedChannel and saved.PostId = Post.Id
left join ChannelItem starred on starred.ChannelId = u.StarredChannel and starred.PostId = Post.Id
left join ReadHistory on ReadHistory.PostId = Post.Id and ReadHistory.UserId = @UserId
left join Popularity pop on pop.PostId = Post.Id
where Post.Created < @Timestamp AND ReadHistory.Timestamp IS NULL) as TBL where TBL.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+@PageSize)
end
