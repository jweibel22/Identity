angular.module('inspire')
    .controller('HomeController', ['$scope', '$http', '$stateParams', 'homePromise','userPromise',
        function ($scope, $http, $stateParams, homePromise, userPromise) {

            $scope.contents = homePromise.data;
            $scope.user = userPromise.data;

            for (var i = 0; i < $scope.contents.TagCloud.length; i++) {
                $scope.contents.TagCloud[i].link = "#/search?query=" + $scope.contents.TagCloud[i].text;
            }
        }]);