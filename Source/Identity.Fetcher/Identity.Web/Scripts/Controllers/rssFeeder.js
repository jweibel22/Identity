angular.module('inspire')
    .controller('RssFeederController', ['$scope', '$http', '$stateParams', 'rssFeederService', 'rssFeederPromise',
        function ($scope, $http, $stateParams, rssFeederService, rssFeederPromise) {

            $scope.rssFeeder = rssFeederPromise.data;

            $scope.editRssFeeder = function() {

                //{ Id: $scope.rssFeeder.Id, Url: $scope.rssFeeder.Id, Tags: $scope.tags }
                rssFeederService.update($scope.rssFeeder);
            }

        }]);