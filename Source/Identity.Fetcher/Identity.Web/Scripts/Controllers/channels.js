angular.module('inspire')
    .controller('ChannelsController', ['$scope', '$http', '$stateParams', '$modal', '$window', '$filter', 'userPromise', 'channelService',
        function ($scope, $http, $stateParams, $modal, $window, $filter, userPromise, channelService) {

            $scope.user = userPromise.data;


            function createTreeViewNode(channel) {

                //var isExpanded = $filter('filter')(expandedNodes, { nodeId: channel.Id }, true)[0];                

                var result = {
                    text: channel.Name,
                    href: "#/home/" + channel.Id
                };

                if (channel.UnreadCount > 0) {
                    result.tags = [channel.UnreadCount];
                }

                //if (isExpanded) {
                //    result.state = { expanded: true };
                //}

                if (channel.Subscriptions.length > 0) {
                    result.nodes = channel.Subscriptions.map(createTreeViewNode);
                }

                return result;
            }            

            $scope.$watch('user.Owns', function (newValue, oldValue, scope) {
                console.log("change detected");
                
                //var expandedNodes = $('#tree').treeview('getExpanded');
                

                                
                //for (var i = 0; i < $scope.tree.length; i++) {

                //    var x = $filter('filter')(expandedNodes, { nodeId: $scope.tree[i].nodeId }, true)[0];

                //    if (x) {
                //        $scope.tree[i].state = { expanded: x.state.expanded }
                //    }
                    
                //    if ($scope.tree[i].nodes) {
                //        for (var j = 0; j < $scope.tree[i].nodes.length; j++) {

                //            var y = $filter('filter')(expandedNodes, { nodeId: $scope.tree[i].nodes[j].nodeId }, true)[0];
                //            if (y) {
                //                $scope.tree[i].nodes[j].state = { expanded: y.state.expanded }
                //            }
                            
                //        }
                //    }
                //}

                //$('#tree').treeview({ data: $scope.tree, levels: 1, enableLinks: true, showTags: true });

            }, true);


            $scope.$watchCollection('user.Owns', function (newArray) {
                console.log('collection changed');

                $scope.tree = $scope.user.Owns.map(createTreeViewNode);
                $('#tree').treeview({ data: $scope.tree, levels: 1, enableLinks: true, showTags: true });
            });

            //var tree = $scope.user.Owns.map(recX);
            //$('#tree').treeview({ data: tree, levels: 1, enableLinks: true, showTags: true});

            $scope.totalUnreadCount = function(channel) {
                var result = channel.UnreadCount;

                for (var i = 0; i < channel.Subscriptions.length; i++) {
                    result += channel.Subscriptions[i].UnreadCount;
                }

                return result;
            }

        }]);