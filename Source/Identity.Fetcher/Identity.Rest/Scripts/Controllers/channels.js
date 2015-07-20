angular.module('inspire')
    .controller('ChannelsController', [ '$scope', '$http', '$stateParams', 'channelService',
        function($scope, $http, $stateParams, channelService){

            $scope.channels = channelService.channels;


        }]);