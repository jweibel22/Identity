angular.module('inspire').directive("stream", ['$window', '$location', '$rootScope', '$modal', '$filter', '$sce', 'postService', 'userService', 'channelSelectorService', function ($window, $location, $rootScope, $modal, $filter, $sce, postService, userService, channelSelectorService) {

        return {
            restrict: 'E',
            scope: {
                posts: "=",
                user: "=",
                channel: "=",
                showonlyunread: "=",
                showcontrols: "=",
                autoloadonscroll: "=",
                displaySettingsChanged: "&"
            },
            templateUrl: 'Content/templates/stream.html',
            controller: function ($scope) {
                
                $scope.loading = false;

                $scope.sortTypes = ["Popularity", "Added"];
                $scope.selectedSortType = $scope.channel ? $scope.channel.DisplaySettings.OrderBy : "Added";
                $scope.reverse = true;

                $scope.listTypes = ["Full", "List", "Titles"];
                $scope.selectedListType = $scope.channel ? $scope.channel.DisplaySettings.ListType : "Titles";

                $scope.channelOwned = $scope.channel && $filter('filter')($scope.user.Owns, { Id: $scope.channel.Id }, true).length > 0;

                $scope.readHistory = [];
                $scope.initialUnread = 0;

                $scope.dndEnabled = $scope.channel ? $scope.channel.DisplaySettings.DraggingEnabled : false;

                $scope.internalPosts = [];
                angular.copy($scope.posts, $scope.internalPosts);

                $scope.groupedPosts = groupByCluster([], $scope.internalPosts);

                $scope.$watchCollection('posts', function (newArray) {
                    if (newArray.length > 0) {
                        var toAppend = filterDuplicates($scope.internalPosts, newArray);
                        $scope.appendPosts($scope.internalPosts, toAppend);
                        $scope.groupedPosts = groupByCluster($scope.groupedPosts, toAppend, true);
                    }                    
                });

                function groupByCluster(result, posts, prepend) {

                    var processedClusters = [];

                    for (var i = 0; i < posts.length; i++) {

                        var post = posts[i];

                        if (isEmpty(post)) {
                            continue;
                        }

                        if (post.ClusterId) {
                            if (processedClusters.indexOf(post.ClusterId) == -1) {
                                processedClusters.push(post.ClusterId);
                                if (prepend) {
                                    result.splice(0, 0, { Posts: $filter('filter')(posts, { ClusterId: post.ClusterId }, true) });
                                } else {
                                    result.push({ Posts: $filter('filter')(posts, { ClusterId: post.ClusterId }, true) });
                                }                                
                            }
                        } else {
                            if (prepend) {
                                result.splice(0, 0, { Posts: [post] });
                            } else {
                                result.push({ Posts: [post] });
                            }
                            
                        }
                    }

                    return result;
                }

                function isEmpty(post) {
                    return post.Id === -1;
                }
               
                $scope.publishOnChannelWindowdata = {
                    user: $scope.user,
                    channels: $scope.user.Owns,
                    selectedChannel: ''
                }
                
                $scope.changeSortBy = function(sortBy) {
                    $scope.selectedSortType = sortBy;
                    $scope.onDisplaySettingsChanged();
                    $scope.reloadPosts();                    
                }

                $scope.changeListBy = function (listBy) {
                    $scope.selectedListType = listBy;
                    $scope.onDisplaySettingsChanged();
                    $scope.reloadPosts();                    
                }

                $scope.changeDraggingEnabled = function () {
                    $scope.onDisplaySettingsChanged();
                }

                $scope.onDisplaySettingsChanged = function () {

                    $scope.displaySettingsChanged({
                        settings: {
                            ShowOnlyUnread: $scope.showonlyunread,
                            OrderBy: $scope.selectedSortType,
                            ListType: $scope.selectedListType,
                            DraggingEnabled: $scope.dndEnabled
                        }
                    });
                }

                $scope.showOnlyUnreadChanged = function () {
                    $scope.onDisplaySettingsChanged();
                    $scope.reloadPosts();
                }

                function filterDuplicates(list, toAppend) {

                    var result = [];
                    for (var i = 0; i < toAppend.length; i++) {
                        var exists = $filter('filter')(list, { Id: toAppend[i].Id }, true).length > 0;

                        if (!exists) {
                            result.push(toAppend[i]);
                        }
                    }

                    return result;
                }

                $scope.appendPosts = function (list, toAppend) {

                    for (var i = 0; i < toAppend.length; i++) {
                        list.push(toAppend[i]);
                    }
                }

                $scope.postVisible = function (post) {

                    if ($scope.channelId == $scope.user.SavedChannel) {
                        return post.Saved;
                    }
                    else if ($scope.channelId == $scope.user.StarredChannel) {
                        return post.Starred;
                    }
                    else if ($scope.channelId == $scope.user.LikedChannel) {
                        return post.Liked;
                    }
                    else {
                        return true;
                    }
                }

                $scope.visiblePosts = function() {
                 
                    return $filter('filter')($scope.internalPosts, function(p) { return !isEmpty(p); }, true);
                }

                $scope.read = function (posts) {

                    for (var i = 0; i < posts.length; i++) {
                        var post = posts[i];                        

                        if (isEmpty(post)) {
                            continue;
                        }

                        var exists = $filter('filter')($scope.readHistory, { Id: post.Id }, true).length > 0;

                        if (!exists) {
                            console.log("Adding to history: " + post.Title);
                            $scope.readHistory.push(post);
                        }
                    }

                    var pending = $filter('filter')($scope.readHistory, { Read: false }, true);

                    if (pending.length > 0) {
                        console.log("Submitting " + pending.length + " posts");

                        var promise = $scope.channelOwned
                            ? postService.readAndDecrementUnreadCount({ PostIds: pending.map(function (p) { return p.Id }) }, $scope.user.Id, $scope.channel.Id)
                            : postService.read({ PostIds: pending.map(function(p) { return p.Id }) }, $scope.user.Id);

                        promise
                            .success(function() {

                                for (var i = 0; i < posts.length; i++) {
                                    posts[i].Read = true;
                                }

                                if ($scope.channel) {
                                    $scope.channel.UnreadCount = $scope.initialUnread - $filter('filter')($scope.readHistory, { Read: true }, true).length;
                                }                                
                            });
                    }
                }

                $scope.reloadPosts = function() {

                    var pageSize = $scope.selectedListType == 'Titles' ? 60 : 30;

                        if (!$scope.loading) {
                            $scope.loading = true;

                            postService.getFromChannel($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType, pageSize).then(function (data) {
                                angular.copy(data.data, $scope.internalPosts);
                                $scope.groupedPosts = groupByCluster([], $scope.internalPosts);
                                $scope.loading = false;

                                $scope.readHistory = [];
                                $scope.initialUnread = $scope.channel.UnreadCount;

                                for (var i = 0; i < $scope.internalPosts.length; i++) {
                                    if ($scope.internalPosts[i].EmbeddedUrl) {
                                        $scope.internalPosts[i].TrustedEmbeddedUrl = $sce.trustAsResourceUrl($scope.internalPosts[i].EmbeddedUrl);
                                    }
                                }
                            });
                        }
                }

                $scope.loadMorePosts = function () {

                    var pageSize = $scope.selectedListType == 'Titles' ? 60 : 30;

                    if ($scope.autoloadonscroll) {

                            if (!$scope.loading) {
                                $scope.loading = true;

                                postService.loadMorePosts($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType, pageSize).then(function (data) {
                                    var toAppend = filterDuplicates($scope.internalPosts, data.data);
                                    $scope.appendPosts($scope.internalPosts, toAppend);
                                    $scope.groupedPosts = groupByCluster($scope.groupedPosts, toAppend);

                                    $scope.loading = false;

                                    for (var i = 0; i < $scope.internalPosts.length; i++) {
                                        if ($scope.internalPosts[i].EmbeddedUrl) {
                                            $scope.internalPosts[i].TrustedEmbeddedUrl = $sce.trustAsResourceUrl($scope.internalPosts[i].EmbeddedUrl);
                                        }
                                    }
                                });
                            }
                    }
                }

                $scope.savePost = function (post) {

                    if (post.Saved) {
                        postService.deletePost($scope.user.SavedChannel, post);
                    } else {
                        postService.savePost($scope.user.SavedChannel, post);
                    }

                    post.Saved = !post.Saved;
                }

                $scope.likePost = function (post) {
                  
                    if (post.Liked) {
                        postService.deletePost($scope.user.LikedChannel, post);
                    } else {
                        postService.savePost($scope.user.LikedChannel, post);
                    }

                    post.Liked = !post.Liked;
                }

                $scope.starPost = function (post) {

                    if (post.Starred) {
                        postService.deletePost($scope.user.StarredChannel, post);
                    } else {
                        postService.savePost($scope.user.StarredChannel, post);
                    }

                    post.Starred = !post.Starred;
                }

                $scope.loadComments = function (post) {
                    postService.get(post.Id).then(function (data) {
                        var index = $scope.internalPosts.indexOf(post);
                        $scope.internalPosts[index].Comments = data.Comments;
                    });
                }

                $scope.deletePost = function (post) {
                    if ($scope.channel) {
                        postService.deletePost($scope.channel.Id, post)
                            .success(function(post1) {
                                var index = $scope.internalPosts.indexOf(post);
                                if (index > -1) {
                                    console.log("removing post");
                                    $scope.internalPosts.splice(index, 1);
                                }
                            });
                    }
                }

                $scope.strip_tags = function(input, allowed) {
                    allowed = (((allowed || '') + '')
                            .toLowerCase()
                            .match(/<[a-z][a-z0-9]*>/g) || [])
                            .join(''); // making sure the allowed arg is a string containing only tags in lowercase (<a><b><c>)
                    var tags = /<\/?([a-z][a-z0-9]*)\b[^>]*>/gi, commentsAndPhpTags = /<!--[\s\S]*?-->|<\?(?:php)?[\s\S]*?\?>/gi;
                    return input.replace(commentsAndPhpTags, '').replace(tags, function ($0, $1) {
                          return allowed.indexOf('<' + $1.toLowerCase() + '>') > -1 ? $0 : '';
                      });
                }

                $scope.fetchPostContent = function (post) {
                    postService.fetchPostContent(post).then(function (data) {
                        var stripped = $scope.strip_tags(data, '<a><br><p><div><script><img>')
                        post.Description = $sce.trustAsHtml(stripped); //data;
                        //post.Description = $sce.trustAsHtml(data);
                    });
                };

                $scope.publishOnChannel = function (post) {

                    channelSelectorService.selectChannel($scope.user.Owns, function(id) { postService.savePost(id, post); });
                }

                $scope.block = function(tag) {
                    userService.block($scope.user.Id, tag);
                }

                if ($scope.channel) {
                    $scope.reloadPosts();
                }
            }
        };


}]);