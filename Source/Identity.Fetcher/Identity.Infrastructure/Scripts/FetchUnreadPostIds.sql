create procedure [dbo].[FetchUnreadPostIds]
	@UserId bigint,
	@FromIndex int,
	@ChannelId bigint,
	@PageSize int,
	@OrderByColumn int,
	@IsPremium int
as 
begin

--TODO: make sure @PageSize < 150 !
drop table IF EXISTS #MyTempTable
select RowNum, PostId, PostClusterId into #MyTempTable from [ftFetchUnreadPosts](@ChannelId,@UserId,@OrderByColumn,@IsPremium) where RowNum BETWEEN (@FromIndex) AND (@FromIndex+150) order by RowNum
DECLARE @MaxRow int
set @MaxRow = (select Min(RowNum) from #MyTempTable where PostClusterId IS NULL and RowNum >= @FromIndex+@PageSize)
select PostId, RowNum from #MyTempTable where RowNum BETWEEN (@FromIndex) AND (CASE WHEN @MaxRow IS NULL THEN @FromIndex+@PageSize ELSE @MaxRow END) order by RowNum
end