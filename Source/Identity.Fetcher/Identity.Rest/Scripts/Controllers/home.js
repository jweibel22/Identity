angular.module('inspire')
.controller('MainCtrl', [
    '$scope', '$modal', '$http', '$stateParams', '$location', '$filter', 'postService', 'channelService', 'tagService', 'channelPromise', 'userPromise',
    function($scope, $modal, $http, $stateParams, $location, $filter, postService, channelService, tagService, channelPromise, userPromise){

        $scope.user = userPromise.data;        
        $scope.posts = postService.posts[channelPromise.data.Id];

        //we want the $scope.channel to point to the same channel object that is used in other controllers
        $scope.channel = $filter('filter')(channelService.channels, { Id: channelPromise.data.Id }, true)[0];

        $scope.channelFollowed = $filter('filter')($scope.user.FollowsChannels, {Id: $scope.channel.Id}, true).length > 0;
        $scope.channelOwned = $filter('filter')($scope.user.Owns, { Id: $scope.channel.Id }, true).length > 0;
        $scope.showOnlyUnread = true;

        $scope.publishOnChannelWindowdata = {
            user: $scope.user,
            channels: $scope.user.Owns,
            selectedChannel: ''
        }

        $scope.incrementUpvotes = function(post) {
            postService.upvote(post);
        };

        $scope.subscribe = function() {
            channelService.subscribe($scope.channel.Id).success(function(data) {
                $scope.channelFollowed = true;
            });
        };

        $scope.unsubscribe = function() {
            channelService.unsubscribe($scope.channel.Id).success(function(data) {
                console.log("done");
                $scope.channelFollowed = false;
            });
        };

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

        $scope.follows = function(tag) {
            return $scope.user.FollowsTags.indexOf(tag) > -1;
        }

        $scope.follow = function(tag) {
            tagService.follow(tag).success(function(data) {
                $scope.user.FollowsTags.push(tag);
            });
        }

        $scope.loadComments = function(post) {
            postService.get(post.Id).then(function(data) {
                var index = $scope.posts.indexOf(post);
                $scope.posts[index].Comments = data.Comments;
            });
        }

        $scope.postVisible = function (post) {
            
            if ($scope.channel.Id == $scope.user.SavedChannel) {
                return post.Saved;
            }
            else if ($scope.channel.Id == $scope.user.StarredChannel) {
                return post.Starred;
            }
            else if ($scope.channel.Id == $scope.user.LikedChannel) {
                return post.Liked;
            }
            else {
                return true;
            }
        }

        $scope.unfollow = function(tag) {
            tagService.unfollow(tag).success(function(data) {
                $scope.user.FollowsTags.splice($scope.user.FollowsTags.indexOf(tag), 1);
            });
        }

        $scope.deletePost = function(post) {
            postService.deletePost($scope.channel.Id, post)
                .success(function (post1) {
                    var index = $scope.posts.indexOf(post);
                    if (index > -1) {
                        console.log("removing post");
                        $scope.posts.splice(index, 1);
                    }
                });
        }

        $scope.read = function (post) {
            if (!post.Read) {
                postService.read(post.Id, $scope.user.Id)
                    .success(function () {
                        if (!post.Read) {
                            console.log("post " + post.Id + " was read");
                        post.Read = true;
                        $scope.channel.UnreadCount = $scope.channel.UnreadCount - 1;
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

        $scope.showOnlyUnreadChanged = function() {

            postService.getFromChannel($scope.channel.Id, $scope.showOnlyUnread).success(function(data) {
                $scope.posts = postService.posts[$scope.channel.Id];
            });
        }

    }]);

angular.module('inspire').directive("scroll", function ($window) {

    function checkVisible(elm, eval) {
        eval = eval || "visible";
        var vpH = $(window).height(), // Viewport Height
            st = $(window).scrollTop(), // Scroll Top
            y = $(elm).offset().top,
            elementHeight = $(elm).height();

        if (eval == "visible") {
            var res = ((y < (vpH + st)) && (st < (y + elementHeight)));
            //if (res) {console.log("y=" + y + ", vpH=" + vpH + ", st=" + st + ", eH=" + elementHeight);}
            return res;
        }
        if (eval == "above") return ((y < (vpH + st)));
    };

    return function (scope, element, attrs) {
        angular.element($window).bind("scroll", function () {
            
            var divs = $(element).find('div.readable');

            for (var i = 0; i < divs.length; i++) {
                if (checkVisible(divs[i])) {
                    scope.read(scope.posts[i]);                    
                }
            }

            scope.$apply();
        });
    };
});