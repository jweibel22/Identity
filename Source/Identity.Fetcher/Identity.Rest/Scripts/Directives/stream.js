angular.module('inspire').directive("stream", ['$window', '$modal', 'postService', 'feedService', 'userService', function ($window, $modal, postService, feedService, userService) {

        return {
            restrict: 'E',
            scope: {
                posts: "=",
                user: "=",
                channel: "=",
                showonlyunread: "="
            },
            templateUrl: 'Content/templates/stream.html',
            controller: function ($scope) {
                
                $scope.loading = false;

                $scope.sortTypes = ["Popularity", "Added"];
                $scope.selectedSortType = "Added";
                $scope.reverse = true;

                $scope.publishOnChannelWindowdata = {
                    user: $scope.user,
                    channels: $scope.user.Owns,
                    selectedChannel: ''
                }

                $scope.changeSortBy = function(sortBy) {
                    $scope.selectedSortType = sortBy;
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

                    if (!$scope.channel) {
                        if (!$scope.loading) {
                            $scope.loading = true;

                            feedService.getFeed($scope.selectedSortType).then(function (data) {
                                $scope.loading = false;
                            });
                        }
                    } else {
                        if (!$scope.loading) {
                            $scope.loading = true;

                            postService.getFromChannel($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType).then(function (data) {
                                $scope.loading = false;
                            });
                        }
                    }
                }

                $scope.loadMorePosts = function () {

                    if (!$scope.channel) {
                        if (!$scope.loading) {
                            $scope.loading = true;

                            feedService.loadMorePosts($scope.selectedSortType).then(function (data) {
                                $scope.loading = false;
                            });
                        }
                    } else {
                        if (!$scope.loading) {
                            $scope.loading = true;

                            postService.loadMorePosts($scope.channel.Id, $scope.showonlyunread, $scope.selectedSortType).then(function (data) {
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

                    $modal.open({
                        templateUrl: 'channelSelector.html',
                        backdrop: true,
                        windowClass: 'modal',
                        controller: function ($scope, $modalInstance, windowdata) {
                            $scope.windowdata = windowdata;

                            $scope.submit = function () {

                                if (!$scope.windowdata.selectedChannel || $scope.windowdata.selectedChannel === '') {
                                    $modalInstance.dismiss('cancel');
                                    return;
                                }

                                postService.savePost($scope.windowdata.selectedChannel.Id, post);

                                $modalInstance.dismiss('cancel');
                            }
                            $scope.cancel = function () {
                                $modalInstance.dismiss('cancel');
                            };
                        },
                        resolve: {
                            windowdata: function () {
                                return $scope.publishOnChannelWindowdata;
                            }
                        }
                    });
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