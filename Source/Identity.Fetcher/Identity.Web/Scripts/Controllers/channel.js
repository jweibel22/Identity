angular.module('inspire')
.controller('ChannelController', [
    '$scope', '$modal', '$http', '$stateParams', '$location', 'ngSettings', '$filter', 'postService', 'channelService', 'tagService', 'channelSelectorService', 'channelPromise', 'userPromise', 
    function($scope, $modal, $http, $stateParams, $location, ngSettings, $filter, postService, channelService, tagService, channelSelectorService, channelPromise, userPromise){

        $scope.findChannel = function (channelId) {

            var x = $filter('filter')($scope.user.Owns, { Id: channelId }, true)[0];

            if (x) {
                return x;
            } 
            return null;
        }


        $scope.user = userPromise.data;        
        $scope.posts = [];

        $scope.rssUrl = ngSettings.baseUrl + "/Api/Channel/" + channelPromise.data.Id + "/Rss";

        $scope.showChannelCloud = channelPromise.data.Subscriptions.length > 0;
        //we want the $scope.channel to point to the same channel object that is used in the channel list, such that the unread counter gets updated
        var x = $scope.findChannel(channelPromise.data.Id);
        $scope.channel = x ? x : channelPromise.data;
        $scope.channel.DisplaySettings = channelPromise.data.DisplaySettings; //Dirty hack, please fix this! The channels from the channel list don't have the DisplaySettings!!

        $scope.channel.Scores = channelPromise.data.Scores; //TODO: find a better way!
        $scope.channel.Statistics = channelPromise.data.Statistics; //TODO: find a better way!

        $scope.channelFollowed = $filter('filter')($scope.user.FollowsChannels, {Id: $scope.channel.Id}, true).length > 0;
        $scope.channelOwned = $filter('filter')($scope.user.Owns, { Id: $scope.channel.Id }, true).length > 0;
        $scope.showOnlyUnread = $scope.channel.DisplaySettings.ShowOnlyUnread;

        for (var i = 0; i < $scope.channel.Scores.length; i++) {
            $scope.channel.Scores[i].link = "#/home/" + $scope.channel.Scores[i].id;
        }

        $("body").scrollTop(0);

        $scope.addLinkWindowdata = {
            user: $scope.user,
            channel: $scope.channel,
            link: '',
            linktags: '', 
            posts : $scope.posts
        }

        $scope.addPostWindowdata = {
            user: $scope.user,
            channel: $scope.channel,
            title: '',
            description: '',
            tags: '',
            posts: $scope.posts
        }

        $scope.addFeederWindowdata = {
            user: $scope.user,
            channel: $scope.channel,
            title: '',
            description: '',
            feedTypes : ["Rss", "Twitter"],
            feedType : "Rss"
        }

        //$scope.subscribe = function () {
        //    channelService.subscribe($scope.channel.Id).success(function (data) {
        //        $scope.channelFollowed = true;

        //        var subscriptionChannel = $filter('filter')($scope.user.Owns, { Id: $scope.user.SubscriptionChannel }, true)[0];
        //        subscriptionChannel.Subscriptions.push($scope.channel);
        //    });
        //};


        //$scope.unsubscribe = function () {
        //    channelService.unsubscribe($scope.channel.Id).success(function (data) {
        //        $scope.channelFollowed = false;

        //        var subscriptionChannel = $filter('filter')($scope.user.Owns, { Id: $scope.user.SubscriptionChannel }, true)[0];
        //        var channelToRemove = $filter('filter')(subscriptionChannel.Subscriptions, { Id: $scope.channel.Id }, true)[0];
        //        var index = subscriptionChannel.Subscriptions.indexOf(channelToRemove);
        //        if (index > -1) {
        //            subscriptionChannel.Subscriptions.splice(index, 1);
        //        }                
        //    });
        //};

        $scope.createLink = function () {
            channelSelectorService.selectChannel($scope.user.Owns, function (id) {
                channelService.addSubscription(id, $scope.channel.Id);
            });
        };


        $scope.markAllAsRead = function () {
            channelService.markAllAsRead($scope.channel.Id).success(function (data) {
                for (var post in $scope.posts) {
                    post.Read = true;
                }
                $scope.channel.UnreadCount = 0;
            });
        };

        $scope.leave = function () {
            channelService.leave($scope.channel.Id).success(function (data) {
                $scope.channelOwned = false;
            });
        };

        $scope.delete = function () {
            channelService.delete($scope.channel).success(function (data) {

                var index = $scope.user.Owns.indexOf($scope.channel);
                if (index > -1) {
                    $scope.user.Owns.splice(index, 1);
                }

                $location.replace().path('#/home');
            });
        };


        $scope.displaySettingsChanged = function (settings) {
            channelService.updateDisplaySettings($scope.channel.Id, $scope.user.Id, settings);
        };

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
                        }, $scope.windowdata.channel.Id).then(function(data) {
                            windowdata.posts.splice(0, 0, data.data);
                        });

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
                            Title: $scope.windowdata.title,
                            Description: $scope.windowdata.description,
                            Uri: null,
                            Type: "userpost",
                            Tags: $scope.windowdata.tags ? $scope.windowdata.tags.split(' ') : null,
                            Created: new Date()
                        }, $scope.windowdata.channel.Id).then(function (data) {
                            windowdata.posts.splice(0,0,data.data);
                        });

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

        $scope.addFeeder = function () {

            $modal.open({
                templateUrl: 'addFeeder.html',
                backdrop: true,
                windowClass: 'modal',
                controller: function ($scope, $modalInstance, windowdata) {
                    $scope.windowdata = windowdata;

                    $scope.submit = function () {

                        if (!$scope.windowdata.rssfeederUrl || $scope.windowdata.rssfeederUrl === '') {
                            $modalInstance.dismiss('cancel');
                            return;
                        }

                        channelService.addFeed($scope.windowdata.channel.Id, $scope.windowdata.rssfeederUrl, $scope.windowdata.feedType).then(function (res) {
                            //TODO: show 'feed added' dialog + refresh stream
                        });

                        $modalInstance.dismiss('cancel');
                    }
                    $scope.cancel = function () {
                        $modalInstance.dismiss('cancel');
                    };
                    $scope.changeFeedType = function (feedType) {
                        $scope.windowdata.feedType = feedType;
                    }
                },
                resolve: {
                    windowdata: function () {
                        return $scope.addFeederWindowdata;
                    }
                }
            });
        };

    }]);
