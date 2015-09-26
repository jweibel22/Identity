angular.module('inspire')
    .controller('ReadHistoryController', [
        '$scope', '$http', '$stateParams', 'postService', 'userPromise',
        function ($scope, $http, $stateParams, postService, userPromise) {

            $scope.user = userPromise.data;
            $scope.posts = postService.posts;
        }]);