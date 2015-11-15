﻿angular.module('inspire').directive("stream", ['$window', '$location', '$rootScope', '$modal', '$filter', '$sce', 'postService', 'userService', 'channelSelectorService', function ($window, $location, $rootScope, $modal, $filter, $sce, postService, userService, channelSelectorService) {

        return {
            restrict: 'E',
            scope: {
                posts: "=",
                user: "=",
                channel: "=",
                showonlyunread: "=",
                showcontrols: "=",
                autoloadonscroll: "=",
            },
            templateUrl: 'Content/templates/stream.html',
            controller: function ($scope) {
                
                $scope.loading = false;

                $scope.sortTypes = ["Popularity", "Added"];
                $scope.selectedSortType = $scope.channel ? $scope.channel.OrderBy : "Added";
                $scope.reverse = true;

                $scope.listTypes = ["Full", "List"];
                $scope.selectedListType = $scope.channel ? $scope.channel.ListType : "Full";

                $scope.channelOwned = $scope.channel && $filter('filter')($scope.user.Owns, { Id: $scope.channel.Id }, true).length > 0;

                $scope.publishOnChannelWindowdata = {
                    user: $scope.user,
                    channels: $scope.user.Owns,
                    selectedChannel: ''
                }
                
                $scope.changeSortBy = function(sortBy) {
                    $scope.selectedSortType = sortBy;
                    $scope.reloadPosts();
                }

                $scope.changeListBy = function (listBy) {
                    $scope.selectedListType = listBy;
                    $scope.reloadPosts();
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

                $scope.read = function (post) {
                    if (!post.Read) {
                        postService.read(post.Id, $scope.user.Id)
                            .success(function () {
                                if (!post.Read) {
                                    post.Read = true;

                                    if ($scope.channel) {
                                        $scope.channel.UnreadCount = $scope.channel.UnreadCount - 1;
                                    }
                                }
                            });
                    }
                }

                $scope.reloadPosts = function() {

                        if (!$scope.loading) {
                            $scope.loading = true;

                            postService.getFromChannel($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType).then(function (data) {
                                angular.copy(data.data.Posts, $scope.posts);
                                $scope.loading = false;

                                for (var i = 0; i < $scope.posts.length; i++) {
                                    if ($scope.posts[i].EmbeddedUrl) {
                                        $scope.posts[i].TrustedEmbeddedUrl = $sce.trustAsResourceUrl($scope.posts[i].EmbeddedUrl);
                                    }
                                }
                            });
                        }
                }

                $scope.loadMorePosts = function () {

                    if ($scope.autoloadonscroll) {

                            if (!$scope.loading) {
                                $scope.loading = true;

                                postService.loadMorePosts($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType).then(function (data) {
                                    angular.copy($scope.posts.concat(data.data.Posts), $scope.posts);
                                    $scope.loading = false;

                                    for (var i = 0; i < $scope.posts.length; i++) {
                                        if ($scope.posts[i].EmbeddedUrl) {
                                            $scope.posts[i].TrustedEmbeddedUrl = $sce.trustAsResourceUrl($scope.posts[i].EmbeddedUrl);
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
                        var index = $scope.posts.indexOf(post);
                        $scope.posts[index].Comments = data.Comments;
                    });
                }

                $scope.deletePost = function (post) {
                    if ($scope.channel) {
                        postService.deletePost($scope.channel.Id, post)
                            .success(function(post1) {
                                var index = $scope.posts.indexOf(post);
                                if (index > -1) {
                                    console.log("removing post");
                                    $scope.posts.splice(index, 1);
                                }
                            });
                    }
                }


                $scope.publishOnChannel = function (post) {

                    channelSelectorService.selectChannel($scope.user.Owns, function(id) { postService.savePost(id, post); });
                }

                if ($scope.channel) {
                    $scope.reloadPosts();
                }
                

                //$scope.incrementUpvotes = function (post) {
                //    postService.upvote(post);
                //};

                //$scope.follows = function (tag) {
                //    return $scope.user.FollowsTags.indexOf(tag) > -1;
                //}

                //$scope.follow = function (tag) {
                //    tagService.follow(tag).success(function (data) {
                //        $scope.user.FollowsTags.push(tag);
                //    });
                //}

                //$scope.unfollow = function (tag) {
                //    tagService.unfollow(tag).success(function (data) {
                //        $scope.user.FollowsTags.splice($scope.user.FollowsTags.indexOf(tag), 1);
                //    });
                //}
            }
        };


}]);