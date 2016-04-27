create procedure [dbo].[FetchPostIds]
	@UserId bigint,
	@FromIndex int,
	@ChannelId bigint,
	@PageSize int,
	@OrderByColumn int
as 
begin
with cte as 
(
    select @ChannelId as Id
    union all
    select t.ChildId as Id from cte 
        inner join [ChannelLink] t on cte.Id = t.Parentid
)

select PostId from (
select AllPosts.*, 
ROW_NUMBER() OVER (ORDER BY CASE @OrderByColumn
    WHEN 1 THEN DATEDIFF(second,{d '1970-01-01'},AllPosts.Added)
    WHEN 2 THEN AllPosts.Popularity
    END desc) AS RowNum from (


select Count(ci.PostId) as Popularity, ci.PostId, Min(ci.Created) as Added from ChannelItem ci
inner join cte on ci.ChannelId = cte.Id
group by ci.PostId) as AllPosts) as Paged
  where Paged.RowNum BETWEEN (@FromIndex+1) AND (@FromIndex+@PageSize)
  order by Paged.RowNum
  end