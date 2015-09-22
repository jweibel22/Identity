angular.module('inspire')
    .controller('ProfileController', ['$scope', '$http', '$stateParams', 'channelService', 'userPromise', 'profilePromise', 'channelSelectorService',
        function ($scope, $http, $stateParams, channelService, userPromise, profilePromise, channelSelectorService) {

            $scope.user = userPromise.data;
            $scope.profile = profilePromise.data;
            $scope.channels = channelService.channels;

            for (var i = 0; i < $scope.profile.TagCloud.length; i++) {
                $scope.profile.TagCloud[i].link = "#/search?query=" + $scope.profile.TagCloud[i].text;
            }

            $scope.grantaccess = function() {
                channelSelectorService.selectChannel($scope.user.Owns, function(channelId) {
                    channelService.grant(channelId, $scope.profile.Id);
                });
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