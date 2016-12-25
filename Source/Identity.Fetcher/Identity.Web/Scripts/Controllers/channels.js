angular.module('inspire')
    .controller('ChannelsController', ['$scope', '$http', '$stateParams', '$modal', '$window', '$filter', 'userPromise', 'channelService',
        function ($scope, $http, $stateParams, $modal, $window, $filter, userPromise, channelService) {

            $scope.user = userPromise.data;

            var treeInitialized = false;

            $scope.showMenu = function() {
                return $window.innerWidth > 1000;
            }

            function tagsEqual(n1, n2) {

                var n1Ok = n1.tags && n1.tags.length == 1;
                var n2Ok = n2.tags && n2.tags.length == 1;

                if (!n1Ok && !n2Ok)
                    return true;
                else if (n1Ok && n2Ok)
                    return n1.tags[0] == n2.tags[0];
                else
                    return false;
           }

            function createTreeViewNode(channel) {

                var result = {
                    uid: channel.Id,
                    text: channel.Name,
                    href: "#/home/" + channel.Id
                };

                if (channel.UnreadCount > 0) {
                    result.tags = [channel.UnreadCount];
                }

                if (channel.Subscriptions.length > 0) {
                    result.nodes = channel.Subscriptions.map(createTreeViewNode);
                }

                return result;
            }            

            $scope.$watch('user.Owns', function (newValue, oldValue, scope) {

                if (!treeInitialized)
                    return;

                var treeNodes = $('#tree').treeview('getNodes');

                for (var i = 0; i < treeNodes.length; i++) {
                    var node = treeNodes[i];

                    if (!node.parentId) {
                        
                        var x = $filter('filter')($scope.user.Owns, { Id: node.uid }, true)[0];

                        if (x) {
                            var newNode = createTreeViewNode(x);

                            if (!tagsEqual(newNode, node)) {
                                newNode.state = node.state; //TODO: copying of the state from old nodes to new nodes should be done recursively on all children
                                $('#tree').treeview('updateNode', [node, newNode, { silent: true }]);
                            }
                        }
                    }
                }
                
            }, true);


            $scope.$watchCollection('user.Owns', function (newArray) {
                console.log('collection changed');

                $scope.tree = $scope.user.ChannelMenuItems.map(createTreeViewNode);
                $('#tree').treeview({
                    data: $scope.tree,
                    levels: 1,
                    enableLinks: true,
                    showTags: true,

                    onNodeSelected: function (event, data) {
                        $window.location.href = data.href;
                    }
                });

                treeInitialized = true;
            });

            $scope.totalUnreadCount = function(channel) {
                var result = channel.UnreadCount;

                for (var i = 0; i < channel.Subscriptions.length; i++) {
                    result += channel.Subscriptions[i].UnreadCount;
                }

                return result;
            }

        }]);