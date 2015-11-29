angular.module('inspire')
    .controller('AllChannelsController', ['$scope', '$http', '$stateParams', 'channelService',
        function ($scope, $http, $stateParams, channelService) {

            $scope.channels = channelService.channels;


        }]);