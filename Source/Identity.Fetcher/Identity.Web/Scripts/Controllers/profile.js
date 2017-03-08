angular.module('inspire')
    .controller('ProfileController', ['$scope', '$http', '$stateParams', 'channelService', 'userPromise', 'profilePromise', 'channelSelectorService', 'userService',
        function ($scope, $http, $stateParams, channelService, userPromise, profilePromise, channelSelectorService, userService) {

            $scope.user = userPromise.data;
            $scope.profile = profilePromise.data;
            //$scope.channels = channelService.channels;

            for (var i = 0; i < $scope.profile.TagCloud.length; i++) {
                $scope.profile.TagCloud[i].link = "#/searchByTag?query=" + $scope.profile.TagCloud[i].text;
            }

            $scope.grantaccess = function() {
                channelSelectorService.selectChannel($scope.user.Owns, function(channelId) {
                    channelService.grant(channelId, $scope.profile.Id);
                });
            }

            $scope.saveChanges = function () {
                userService.update($scope.user);
            }
        }]);