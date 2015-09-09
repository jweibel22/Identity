angular.module('inspire')
    .controller('UserSearchResultsController', ['$scope', '$http', '$stateParams', 'userService',
        function ($scope, $http, $stateParams, userService) {

            $scope.users = userService.searchResult;

        }]);