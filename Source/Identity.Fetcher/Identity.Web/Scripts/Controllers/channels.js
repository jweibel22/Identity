angular.module('inspire')
    .controller('ChannelsController', ['$scope', '$http', '$stateParams', 'userPromise',
        function ($scope, $http, $stateParams, userPromise) {

            $scope.user = userPromise.data;


        }]);