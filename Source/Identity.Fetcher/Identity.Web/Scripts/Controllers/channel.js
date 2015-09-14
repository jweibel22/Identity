angular.module('inspire')
.controller('ChannelController', [
    '$scope', '$modal', '$http', '$stateParams', '$location', '$filter', 'postService', 'channelService', 'tagService', 'channelPromise', 'userPromise', 
    function($scope, $modal, $http, $stateParams, $location, $filter, postService, channelService, tagService, channelPromise, userPromise){

        $scope.user = userPromise.data;        
        $scope.posts = postService.posts[channelPromise.data.Id];

        //we want the $scope.channel to point to the same channel object that is used in the channel list, such that the unread counter gets updated
        var x = $filter('filter')($scope.user.Owns, { Id: channelPromise.data.Id }, true)[0];
        $scope.channel = x ? x : channelPromise.data;

        $scope.channel.TagCloud = channelPromise.data.TagCloud; //TODO: find a better way!

        $scope.channelFollowed = $filter('filter')($scope.user.FollowsChannels, {Id: $scope.channel.Id}, true).length > 0;
        $scope.channelOwned = $filter('filter')($scope.user.Owns, { Id: $scope.channel.Id }, true).length > 0;
        $scope.showOnlyUnread = true;

        for (var i = 0; i < $scope.channel.TagCloud.length; i++) {
            $scope.channel.TagCloud[i].link = "#/search?query=" + $scope.channel.TagCloud[i].text;
        }

        $scope.addLinkWindowdata = {
            user: $scope.user,
            channel: $scope.channel,
            link: '',
            linktags: ''
        }

        $scope.addPostWindowdata = {
            user: $scope.user,
            channel: $scope.channel,
            title: '',
            description: '',
            tags: ''
        }

        $scope.subscribe = function () {
            channelService.subscribe($scope.channel.Id).success(function (data) {
                $scope.channelFollowed = true;
            });
        };

        $scope.unsubscribe = function () {
            channelService.unsubscribe($scope.channel.Id).success(function (data) {
                console.log("done");
                $scope.channelFollowed = false;
            });
        };

        $scope.leave = function () {
            channelService.leave($scope.channel.Id).success(function (data) {
                $scope.channelOwned = false;
            });
        };

        $scope.showOnlyUnreadChanged = function() {

            postService.getFromChannel($scope.channel.Id, $scope.showOnlyUnread).success(function(data) {
                $scope.posts = postService.posts[$scope.channel.Id];
            });
        }

        $scope.addLink = function () {

            $modal.open({
                templateUrl: 'addLinkModal.html',
                backdrop: true,
                windowClass: 'modal',
                controller: function ($scope, $modalInstance, windowdata) {
                    $scope.windowdata = windowdata;

                    $scope.submit = function () {

                        if (!$scope.windowdata.link || $scope.windowdata.link === '') {
                            $modalInstance.dismiss('cancel');
                            return;
                        }

                        postService.create({
                            Title: null,
                            Description: null,
                            Uri: $scope.windowdata.link,
                            Type: "link",
                            Tags: $scope.windowdata.linktags ? $scope.windowdata.linktags.split(' ') : null,
                            Created: new Date()
                        }, $scope.windowdata.channel.Id);

                        $modalInstance.dismiss('cancel');
                    }
                    $scope.cancel = function () {
                        $modalInstance.dismiss('cancel');
                    };
                },
                resolve: {
                    windowdata: function () {
                        return $scope.addLinkWindowdata;
                    }
                }
            });
        };

        $scope.addPost = function () {

            $modal.open({
                templateUrl: 'addPostModal.html',
                backdrop: true,
                windowClass: 'modal',
                controller: function ($scope, $modalInstance, windowdata) {
                    $scope.windowdata = windowdata;

                    $scope.submit = function () {

                        if (!$scope.windowdata.title || $scope.windowdata.title === '') {
                            $modalInstance.dismiss('cancel');
                            return;
                        }

                        postService.create({
                            title: $scope.windowdata.title,
                            description: $scope.windowdata.description,
                            uri: guid(),
                            type: "userpost",
                            tags: $scope.windowdata.tags ? $scope.windowdata.tags.split(' ') : null,
                            created: new Date()
                        }, $scope.windowdata.channel.Id);

                        $modalInstance.dismiss('cancel');
                    }
                    $scope.cancel = function () {
                        $modalInstance.dismiss('cancel');
                    };
                },
                resolve: {
                    windowdata: function () {
                        return $scope.addPostWindowdata;
                    }
                }
            });
        };

    }]);
