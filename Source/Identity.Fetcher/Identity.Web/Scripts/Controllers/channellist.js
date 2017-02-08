angular.module('inspire')
    .controller('ChannelListController', [
        '$scope', '$http', '$stateParams',  'channelsPromise',
        function ($scope, $http, $stateParams, channelsPromise) {
            $scope.channels = channelsPromise.data;
        }]);