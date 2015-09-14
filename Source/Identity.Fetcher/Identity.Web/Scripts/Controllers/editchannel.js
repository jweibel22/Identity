angular.module('inspire')
    .controller('EditChannelController', ['$scope', '$http', '$stateParams', 'channelService',
        function ($scope, $http, $stateParams, channelService) {

            $scope.channel = channelService.editchannel;

            $scope.addRssFeeder = function() {
                $scope.channel.RssFeeders.push({ Url: $scope.rssfeederUrl });

                channelService.update($scope.channel).then(function(res) {
                    $scope.channel = res.data;
                });
            }

            $scope.removeRssFeeder = function (rssFeeder) {

                var index = $scope.channel.RssFeeders.indexOf(rssFeeder);
                if (index > -1) {
                    $scope.channel.RssFeeders.splice(index, 1);
                }
                
                channelService.update($scope.channel).then(function (res) {
                    $scope.channel = res.data;
                });
            }

        }]);