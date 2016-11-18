angular.module('inspire')
    .controller('EditChannelController', ['$scope', '$http', '$stateParams', 'channelService',
        function ($scope, $http, $stateParams, channelService) {

            $scope.channel = channelService.editchannel;

            $scope.feedTypes = ["Rss", "Twitter"];
            $scope.feedType = "Rss";

            $scope.addRssFeeder = function () {

                channelService.addFeed($scope.channel.Id, $scope.rssfeederUrl, $scope.feedType).then(function (res) {
                    
                });
            }

            $scope.changeFeedType = function (feedType) {
                $scope.feedType = feedType;
            }

            $scope.removeSubscription = function (child) {

                var index = $scope.channel.Subscriptions.indexOf(child);
                if (index > -1) {
                    $scope.channel.Subscriptions.splice(index, 1);
                }
            }

            $scope.saveChanges = function() {

                channelService.update($scope.channel).then(function (res) {
                    $scope.channel = res.data;
                });
            }

        }]);