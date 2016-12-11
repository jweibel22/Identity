create procedure [dbo].[FetchPostIds]
	@UserId bigint,
	@FromIndex int,
	@ChannelId bigint,
	@PageSize int,
	@OrderByColumn int
as 
begin

--TODO: make sure @PageSize < 150 !
drop table IF EXISTS #MyTempTable
select RowNum, PostId, PostClusterId into #MyTempTable from [ftFetchPosts](@ChannelId,@UserId,@OrderByColumn) where RowNum BETWEEN (@FromIndex) AND (@FromIndex+150) order by RowNum
DECLARE @MaxRow int
set @MaxRow = (select Min(RowNum) from #MyTempTable where PostClusterId IS NULL and RowNum >= @FromIndex+@PageSize)
select PostId from #MyTempTable where RowNum BETWEEN (@FromIndex) AND (@MaxRow) order by RowNum
end