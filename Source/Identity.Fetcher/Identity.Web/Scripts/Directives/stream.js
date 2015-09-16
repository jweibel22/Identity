angular.module('inspire').directive("stream", ['$window', '$location', '$rootScope', '$modal', 'postService', 'userService', 'channelSelectorService', function ($window, $location, $rootScope, $modal, postService, userService, channelSelectorService) {

        return {
            restrict: 'E',
            scope: {
                posts: "=",
                user: "=",
                channel: "=",
                showonlyunread: "=",
                showcontrols: "=",
                autoloadonscroll: "="              
            },
            templateUrl: 'Content/templates/stream.html',
            controller: function ($scope) {
                
                $scope.loading = false;

                $scope.sortTypes = ["Popularity", "Added"];
                $scope.selectedSortType = "Added";
                $scope.reverse = true;

                $scope.listTypes = ["Full", "List"];
                $scope.selectedListType = "Full";

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
                                    console.log("post " + post.Id + " was read");
                                    post.Read = true;                                    
                                }
                            });
                    }
                }

                $scope.reloadPosts = function() {

                        if (!$scope.loading) {
                            $scope.loading = true;

                            postService.getFromChannel($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType).then(function (data) {
                                $scope.loading = false;
                            });
                        }
                }

                $scope.loadMorePosts = function () {

                    if ($scope.autoloadonscroll) {

                            if (!$scope.loading) {
                                $scope.loading = true;

                                postService.loadMorePosts($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType).then(function(data) {
                                    $scope.loading = false;
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