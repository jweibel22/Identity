angular.module('inspire')
    .controller('HomeController', ['$scope', '$http', '$sce', '$stateParams', 'homePromise', 'userPromise',
        function ($scope, $http, $sce, $stateParams, homePromise, userPromise) {

            $scope.contents = homePromise.data;
            $scope.user = userPromise.data;

            for (var i = 0; i < $scope.contents.TagCloud.length; i++) {
                $scope.contents.TagCloud[i].link = "#/searchByTag?query=" + $scope.contents.TagCloud[i].text;
            }

            for (var i = 0; i < $scope.contents.Posts.length; i++) {

                //TODO: this code throws exceptions in the console, fix it!
                //if ($scope.contents.Posts[i].EmbeddedUrl) {
                //    $scope.contents.Posts[i].TrustedEmbeddedUrl = $sce.trustAsResourceUrl($scope.contents.Posts[i].EmbeddedUrl);
                //}

                $scope.contents.Posts[i].TrustedEmbeddedUrl = null;
            }
        }]);