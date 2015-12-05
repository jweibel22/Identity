angular.module('inspire')
    .controller('ChannelsController', ['$scope', '$http', '$stateParams', 'userPromise',
        function ($scope, $http, $stateParams, userPromise) {

            $scope.user = userPromise.data;

            $scope.totalUnreadCount = function(channel) {
                var result = channel.UnreadCount;

                for (var i = 0; i < channel.Subscriptions.length; i++) {
                    result += channel.Subscriptions[i].UnreadCount;
                }

                return result;
            }
        }]);