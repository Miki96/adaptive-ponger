using SharpLearning.AdaBoost.Learners;
using SharpLearning.Common.Interfaces;
using SharpLearning.CrossValidation.TrainingTestSplitters;
using SharpLearning.DecisionTrees.Learners;
using SharpLearning.DecisionTrees.Models;
using SharpLearning.FeatureTransformations.MatrixTransforms;
using SharpLearning.GradientBoost.Learners;
using SharpLearning.GradientBoost.Models;
using SharpLearning.InputOutput.Csv;
using SharpLearning.InputOutput.Serialization;
using SharpLearning.Metrics.Regression;
using SharpLearning.Neural;
using SharpLearning.Neural.Activations;
using SharpLearning.Neural.Layers;
using SharpLearning.Neural.Learners;
using SharpLearning.Neural.Loss;
using SharpLearning.Neural.Models;
using SharpLearning.RandomForest.Learners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SharpML
{
    //private RegressionNeuralNetModel model;

    private MACHINE_TYPE type;

    private String root = Application.persistentDataPath + "/";

    private ILearner<double> learner;
    private IPredictorModel<double> model;

    public SharpML(MACHINE_TYPE t, string path)
    {
        LoadModel(path);

        // create new
        type = t;
        switch (type)
        {
            case MACHINE_TYPE.PLAYER:
                learner = null;
                break;
            case MACHINE_TYPE.DECISION_TREE:
                learner = new RegressionDecisionTreeLearner();
                break;
            case MACHINE_TYPE.RANDOM_FOREST:
                learner = new RegressionRandomForestLearner();
                break;
            case MACHINE_TYPE.EXTRA_TREES:
                learner = new RegressionExtremelyRandomizedTreesLearner();
                break;


            case MACHINE_TYPE.ADABOOST:
                learner = new RegressionAdaBoostLearner();
                break;

            case MACHINE_TYPE.GRAD_SQUARE:
                learner = new RegressionSquareLossGradientBoostLearner();
                break;
            case MACHINE_TYPE.GRAD_ABSOLUTE:
                learner = new RegressionAbsoluteLossGradientBoostLearner();
                break;
            case MACHINE_TYPE.GRAD_HUBER:
                learner = new RegressionHuberLossGradientBoostLearner();
                break;
            case MACHINE_TYPE.GRAD_QUANTILE:
                learner = new RegressionQuantileLossGradientBoostLearner();
                break;

            case MACHINE_TYPE.NEURAL_NETWORK:
                NeuralNet net = new NeuralNet();
                net.Add(new InputLayer(inputUnits: 4));
                net.Add(new DenseLayer(9, Activation.Relu));
                net.Add(new DenseLayer(9, Activation.Relu));
                net.Add(new SquaredErrorRegressionLayer());
                learner = new RegressionNeuralNetLearner(net
                    , loss: new SquareLoss()
                    , iterations: 100
                    , learningRate: 0.002
                    , batchSize: 128
                );
                break;
        }
    }

    public void Train(string dataName, string savePath)
    {
        var parser = new CsvParser(() => new StreamReader(root + dataName + ".csv"), ',');
        var targetName = "f5";
        var enemyName = "f6";

        try
        {
            // read data
            var observations = parser.EnumerateRows(c => c != targetName && c != enemyName)
                .ToF64Matrix();

            var targets = parser.EnumerateRows(targetName)
                .ToF64Vector();

            //Debug.Log("READ");

            // split data
            //var splitter = new StratifiedTrainingTestIndexSplitter<double>(trainingPercentage: 0.9, seed: 1);
            //var trainingTestSplit = splitter.SplitSet(observations, targets);
            //var trainingSet = trainingTestSplit.TrainingSet;
            //var testSet = trainingTestSplit.TestSet;

            //Debug.Log("SPLIT");

            //model = learner.Learn(trainingSet.Observations, trainingSet.Targets);
            model = learner.Learn(observations, targets);

            //Debug.Log("TRAINED");

            //var met1 = new MeanSquaredErrorRegressionMetric();
            //var met2 = new CoefficientOfDeterminationMetric();
            //var met3 = new MeanAbsolutErrorRegressionMetric();
            //var met4 = new NormalizedGiniCoefficientRegressionMetric();
            //var met5 = new RootMeanSquarePercentageRegressionMetric();

            //var pred = model.Predict(testSet.Observations);
            //Debug.Log(met1.Error(testSet.Targets, pred) + " <-> " + type);
            //Debug.Log(met2.Error(testSet.Targets, pred) + " <-> " + type);
            //Debug.Log(met3.Error(testSet.Targets, pred) + " <-> " + type);
            //Debug.Log(met4.Error(testSet.Targets, pred) + " <-> " + type);
            //Debug.Log(met5.Error(testSet.Targets, pred) + " <-> " + type);

            // NEURAL TEST
            Debug.Log("TRAINED");

            // save 
            SaveModel(savePath);

            // load
            //LoadModel("" + LevelData.instance.stage + "/" + LevelData.instance.playerID + ".model");

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            model = null;
        }
        finally
        {
        }
    }

    public void SaveModel(string path)
    {
        if (type == MACHINE_TYPE.NEURAL_NETWORK)
        {
            var binarySerializer = new GenericBinarySerializer();
            binarySerializer.Serialize<IPredictorModel<double>>(model,
                () => new StreamWriter(root + path));
        }
    }

    public void LoadModel(string path)
    {
        model = null;
        try
        {
            var binarySerializer = new GenericBinarySerializer();
            model = binarySerializer.Deserialize<IPredictorModel<double>>(
                () => new StreamReader(root + path));
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            model = null;
        }
    }

    public double Predict(PongData data)
    {
        if (model == null) return 0;

        double[] observations = new double[] {
            data.ballPosX,
            data.ballPosY,
            data.ballDirX,
            data.ballDirY
        };

        return model.Predict(observations); ;
    }
}
