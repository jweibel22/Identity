angular.module('inspire')
    .controller('ProfileController', ['$scope', '$http', '$stateParams', 'channelService', 'userPromise', 'profilePromise',
        function ($scope, $http, $stateParams, channelService, userPromise, profilePromise) {

            $scope.user = userPromise.data;
            $scope.profile = profilePromise.data;
            $scope.channels = channelService.channels;

            for (var i = 0; i < $scope.profile.TagCloud.length; i++) {
                $scope.profile.TagCloud[i].link = "#/search?query=" + $scope.profile.TagCloud[i].text;
            }

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
                    $scope.profile.Owns.push(data);
                });

                $scope.newChannelName = "";
            }

            $scope.delete = function (channel) {
                channelService.delete(channel).success(function(data) {
                    var index = $scope.profile.Owns.indexOf(channel);
                    if (index > -1) {
                        $scope.profile.Owns.splice(index, 1);
                    }
                });

            }
        }]);