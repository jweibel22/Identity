angular.module('inspire')
    .controller('ChannelSearchResultsController', ['$scope', '$http', '$stateParams', 'channelService',
        function ($scope, $http, $stateParams, channelService) {

            $scope.channels = channelService.searchResult;

        }]);