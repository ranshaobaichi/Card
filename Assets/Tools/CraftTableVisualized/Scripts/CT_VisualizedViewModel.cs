using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace CraftTableVisualized
{
    public class CT_VisualizedLinkedNode
    {
        public Card.CardDescription Card;
        public List<CT_VisualizedLinkedEdge> ToRecipes;
        public List<CT_VisualizedLinkedEdge> FromRecipes;
    }
    
    public class CT_VisualizedLinkedEdge
    {
        public CT_VisualizedLinkedNode FromCard;
        public CT_VisualizedLinkedNode ToCard;
        public CraftTableDB.Recipe Recipe;
    }
    
    [Serializable]
    public class CT_VisualizedViewModel
    {
        public Card.CardDescription curSelectedCardDescription;

        public List<List<CT_VisualizedLinkedNode>> stageNodes;
        public List<List<CT_VisualizedLinkedEdge>> stageEdges;
        
        // Here we suppose that all card types are in the craft table
        public List<Card.CardDescription> cardDescriptions;
        
        private CT_VisualizedModel _model;
        private Dictionary<Card.CardDescription, List<CraftTableDB.Recipe>> _recipesByInputCard;
        private Dictionary<Card.CardDescription, List<CraftTableDB.Recipe>> _recipesByOutputCard;
        private Dictionary<Card.CardDescription, CT_VisualizedLinkedNode> _nodeCache;
        
        public void Init(CT_VisualizedModel model)
        {
            _model = model;
            cardDescriptions = new List<Card.CardDescription>();

            if (_model?.recipes != null)
            {
                foreach (var recipe in _model.recipes)
                {
                    if (recipe.inputCards != null)
                        cardDescriptions.AddRange(recipe.inputCards);
                    if (recipe.outputCards != null)
                        cardDescriptions.AddRange(recipe.outputCards.Select(x => x.cardDescription));
                }
            }
            var order = new Dictionary<Category.CardType, int>{
                { Category.CardType.Resources,0 },
                { Category.CardType.Creatures,1 },
                { Category.CardType.Events,2 },
            };

            cardDescriptions = cardDescriptions .Distinct()
                .OrderBy(c => order[c.cardType])
                .ToList();

            BuildRecipeIndex();
        }

        private void BuildRecipeIndex()
        {
            _recipesByInputCard = new Dictionary<Card.CardDescription, List<CraftTableDB.Recipe>>();
            _recipesByOutputCard = new Dictionary<Card.CardDescription, List<CraftTableDB.Recipe>>();
            if (_model?.recipes == null) return;
            foreach (var recipe in _model.recipes)
            {
                if (recipe.inputCards != null)
                {
                    foreach (var card in recipe.inputCards)
                    {
                        if (!_recipesByInputCard.TryGetValue(card, out var list))
                        {
                            list = new List<CraftTableDB.Recipe>();
                            _recipesByInputCard[card] = list;
                        }
                        list.Add(recipe);
                    }
                }
                if (recipe.outputCards != null)
                {
                    foreach (var drop in recipe.outputCards)
                    {
                        var card = drop.cardDescription;
                        if (!_recipesByOutputCard.TryGetValue(card, out var list))
                        {
                            list = new List<CraftTableDB.Recipe>();
                            _recipesByOutputCard[card] = list;
                        }
                        list.Add(recipe);
                    }
                }
            }
        }
        
        public bool UpdateSelectedCard(Card.CardDescription cardDescription)
        {
            if (curSelectedCardDescription == cardDescription)
            {
                return false;
            }
            curSelectedCardDescription = cardDescription;
            _nodeCache = new Dictionary<Card.CardDescription, CT_VisualizedLinkedNode>();
            var headNode = GetOrCreateNode(cardDescription);
            
            BuildInputRoot(headNode);
            BuildOutputRoot(headNode);

            BuildStagesFromHead(headNode);
            _nodeCache = null;
            return true;
        }

        private CT_VisualizedLinkedNode GetOrCreateNode(Card.CardDescription card)
        {
            if (_nodeCache != null && _nodeCache.TryGetValue(card, out var existing))
                return existing;
            var node = CreateNode(card);
            _nodeCache[card] = node;
            return node;
        }

        private void BuildStagesFromHead(CT_VisualizedLinkedNode headNode)
        {
            var stageNodesByDepth = new Dictionary<int, List<CT_VisualizedLinkedNode>>();
            var stageEdgesByDepth = new Dictionary<int, List<CT_VisualizedLinkedEdge>>();
            var depthByNode = new Dictionary<CT_VisualizedLinkedNode, int>();

            AddStageNode(stageNodesByDepth, depthByNode, headNode, 0);
            BuildLeftStages(headNode, stageNodesByDepth, stageEdgesByDepth, depthByNode);
            BuildRightStages(headNode, stageNodesByDepth, stageEdgesByDepth, depthByNode);

            var orderedDepths = stageNodesByDepth.Keys.OrderBy(x => x).ToList();
            stageNodes = new List<List<CT_VisualizedLinkedNode>>();
            stageEdges = new List<List<CT_VisualizedLinkedEdge>>();

            foreach (var depth in orderedDepths)
            {
                stageNodes.Add(stageNodesByDepth[depth]);
                if (stageEdgesByDepth.TryGetValue(depth, out var edges))
                    stageEdges.Add(edges);
                else
                    stageEdges.Add(new List<CT_VisualizedLinkedEdge>());
            }
        }

        private void BuildLeftStages(
            CT_VisualizedLinkedNode headNode,
            Dictionary<int, List<CT_VisualizedLinkedNode>> stageNodesByDepth,
            Dictionary<int, List<CT_VisualizedLinkedEdge>> stageEdgesByDepth,
            Dictionary<CT_VisualizedLinkedNode, int> depthByNode)
        {
            var queue = new Queue<CT_VisualizedLinkedNode>();
            queue.Enqueue(headNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var depth = depthByNode[node];
                if (node.FromRecipes == null) continue;

                foreach (var edge in node.FromRecipes)
                {
                    var parent = edge.FromCard;
                    var parentDepth = depth - 1;

                    AddStageEdge(stageEdgesByDepth, parentDepth, edge);
                    if (AddStageNode(stageNodesByDepth, depthByNode, parent, parentDepth))
                        queue.Enqueue(parent);
                }
            }
        }

        private void BuildRightStages(
            CT_VisualizedLinkedNode headNode,
            Dictionary<int, List<CT_VisualizedLinkedNode>> stageNodesByDepth,
            Dictionary<int, List<CT_VisualizedLinkedEdge>> stageEdgesByDepth,
            Dictionary<CT_VisualizedLinkedNode, int> depthByNode)
        {
            var queue = new Queue<CT_VisualizedLinkedNode>();
            queue.Enqueue(headNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var depth = depthByNode[node];
                if (node.ToRecipes == null) continue;

                foreach (var edge in node.ToRecipes)
                {
                    var child = edge.ToCard;
                    var childDepth = depth + 1;

                    AddStageEdge(stageEdgesByDepth, depth, edge);
                    if (AddStageNode(stageNodesByDepth, depthByNode, child, childDepth))
                        queue.Enqueue(child);
                }
            }
        }

        private static bool AddStageNode(
            Dictionary<int, List<CT_VisualizedLinkedNode>> stageNodesByDepth,
            Dictionary<CT_VisualizedLinkedNode, int> depthByNode,
            CT_VisualizedLinkedNode node,
            int depth)
        {
            if (depthByNode.ContainsKey(node)) return false;

            depthByNode[node] = depth;
            if (!stageNodesByDepth.TryGetValue(depth, out var list))
            {
                list = new List<CT_VisualizedLinkedNode>();
                stageNodesByDepth[depth] = list;
            }
            list.Add(node);
            return true;
        }

        private static void AddStageEdge(
            Dictionary<int, List<CT_VisualizedLinkedEdge>> stageEdgesByDepth,
            int depth,
            CT_VisualizedLinkedEdge edge)
        {
            if (!stageEdgesByDepth.TryGetValue(depth, out var list))
            {
                list = new List<CT_VisualizedLinkedEdge>();
                stageEdgesByDepth[depth] = list;
            }
            list.Add(edge);
        }

        private void BuildInputRoot(CT_VisualizedLinkedNode rootNode)
        {
            // Right side: single-in, multi-out per node (except head).
            var path = new HashSet<Card.CardDescription> { rootNode.Card };
            BuildRightTree(rootNode, path);
        }

        private void BuildOutputRoot(CT_VisualizedLinkedNode rootNode)
        {
            // Left side: multi-in, single-out per node (except head).
            var path = new HashSet<Card.CardDescription> { rootNode.Card };
            BuildLeftTree(rootNode, path);
        }

        private void BuildRightTree(CT_VisualizedLinkedNode node, HashSet<Card.CardDescription> path)
        {
            var recipes = GetRecipesUsingAsInput(node.Card);
            if (recipes == null) return;
            foreach (var recipe in recipes)
            {
                if (recipe.outputCards == null) continue;
                foreach (var drop in recipe.outputCards)
                {
                    var nextCard = drop.cardDescription;
                    if (path.Contains(nextCard)) continue;

                    var child = GetOrCreateNode(nextCard);
                    if (!TryAddEdge(node, child, recipe)) continue;

                    path.Add(nextCard);
                    BuildRightTree(child, path);
                    path.Remove(nextCard);
                }
            }
        }

        private void BuildLeftTree(CT_VisualizedLinkedNode node, HashSet<Card.CardDescription> path)
        {
            var recipes = GetRecipesProducingAsOutput(node.Card);
            if (recipes == null) return;
            foreach (var recipe in recipes)
            {
                if (recipe.inputCards == null) continue;
                foreach (var prevCard in recipe.inputCards)
                {
                    if (path.Contains(prevCard)) continue;

                    var parent = GetOrCreateNode(prevCard);
                    if (!TryAddEdge(parent, node, recipe)) continue;

                    path.Add(prevCard);
                    BuildLeftTree(parent, path);
                    path.Remove(prevCard);
                }
            }
        }

        private CT_VisualizedLinkedNode CreateNode(Card.CardDescription card)
        {
            return new CT_VisualizedLinkedNode
            {
                Card = card,
                ToRecipes = new List<CT_VisualizedLinkedEdge>(),
                FromRecipes = new List<CT_VisualizedLinkedEdge>()
            };
        }

        /// <summary> 添加一条边，若已存在相同 From/To/Recipe 则返回 false，避免重复边 </summary>
        private bool TryAddEdge(
            CT_VisualizedLinkedNode fromNode,
            CT_VisualizedLinkedNode toNode,
            CraftTableDB.Recipe recipe)
        {
            if (fromNode.ToRecipes != null)
            {
                foreach (var e in fromNode.ToRecipes)
                    if (e.ToCard == toNode && e.Recipe.Equals(recipe)) return false;
            }
            var edge = new CT_VisualizedLinkedEdge
            {
                FromCard = fromNode,
                ToCard = toNode,
                Recipe = recipe
            };
            fromNode.ToRecipes.Add(edge);
            toNode.FromRecipes.Add(edge);
            return true;
        }

        private List<CraftTableDB.Recipe> GetRecipesUsingAsInput(Card.CardDescription card)
        {
            if (_recipesByInputCard == null) return null;
            return _recipesByInputCard.TryGetValue(card, out var list) ? list : null;
        }

        private List<CraftTableDB.Recipe> GetRecipesProducingAsOutput(Card.CardDescription card)
        {
            if (_recipesByOutputCard == null) return null;
            return _recipesByOutputCard.TryGetValue(card, out var list) ? list : null;
        }
    }
}