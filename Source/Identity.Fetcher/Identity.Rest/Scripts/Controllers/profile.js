angular.module('inspire')
    .controller('ProfileController', ['$scope', '$http', '$stateParams', 'channelService', 'userPromise',
        function ($scope, $http, $stateParams, channelService, userPromise) {

            $scope.user = userPromise.data;
            $scope.channels = channelService.channels;


            $scope.makePublic = function(channel) {
                channel.IsPrivate = false;
                channelService.update(channel);
            }

            $scope.makePrivate = function (channel) {
                channel.IsPrivate = true;
                channelService.update(channel);
            }

            $scope.addChannel = function() {
                channelService.create({ Name: $scope.newChannelName }).success(function(data) {
                    $scope.user.Owns.push(data);
                });

                $scope.newChannelName = "";
            }

            $scope.delete = function (channel) {
                channelService.delete(channel).success(function(data) {
                    var index = $scope.user.Owns.indexOf(channel);
                    if (index > -1) {
                        $scope.user.Owns.splice(index, 1);
                    }
                });

            }
        }]);