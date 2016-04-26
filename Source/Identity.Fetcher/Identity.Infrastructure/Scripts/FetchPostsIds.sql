create procedure [dbo].[FetchPostsIds]
	@UserId bigint,
	@FromIndex int,
	@ChannelId bigint,
	@PageSize int
as 
begin
select Id from 
(select Id, ROW_NUMBER() OVER (ORDER BY AllPosts.Created desc) AS RowNum
 from (select Post.Id, Post.Created
  FROM [dbo].[Post]  
inner join ChannelItem ci on ci.PostId = Post.Id
  where ci.ChannelId = @ChannelId
  
  union

  select Post.Id, Post.Created
  FROM [dbo].[Post]  
inner join ChannelItem ci on ci.PostId = Post.Id
left join ChannelLink cl on cl.ChildId = ci.ChannelId  
  where cl.ParentId = @ChannelId

  ) as AllPosts
  ) as PagedPosts
  where PagedPosts.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+@PageSize)
  order by PagedPosts.RowNum


  end