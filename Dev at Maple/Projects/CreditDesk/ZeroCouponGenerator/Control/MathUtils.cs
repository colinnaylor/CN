using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
namespace ZeroCouponGenerator {

    public class Utils
    {
        public static void Log(string message)
        {

            Maple.Logger.Log(message);
            LogManager.GetCurrentClassLogger().Info(message);

        }

    }

    class MathUtils {
        public void PrintMatrix(double[,] mat, string name) {
            Console.WriteLine("\n " + name + ":");
            for (int i = 0; i < mat.GetLength(0); i++) {
                for (int j = 0; j < mat.GetLength(1); j++) {
                    Console.Write("\t" + mat[i, j]);
                }
                Console.WriteLine();
            }
        }

        public void MultiplyMatrix(double[,] mat1, double[,] mat2, out double[,] result) {
            result = new double[mat1.GetLength(0), mat2.GetLength(1)];
            int rows1 = mat1.GetLength(0);
            int rows2 = mat2.GetLength(0);
            int cols1 = mat1.GetLength(1);
            int cols2 = mat2.GetLength(1);
            if (rows1 == cols2) {
                result = new double[rows2, cols1];

                for (int i = 0; i < cols1; i++) {
                    for (int j = 0; j < rows2; j++) {
                        result[j, i] = 0;
                        for (int k = 0; k < rows1; k++) {
                            //Console.WriteLine("{0}  {1}", mat1[k, i], mat2[j, k]);
                            result[j, i] = result[j, i] + mat1[k, i] * mat2[j, k];
                        }
                        //Console.WriteLine("{0}", result[i, j]);
                    }
                }
            } else {
                Console.WriteLine("\n Number of columns in Matrix1 is not equal to Number of rows in Matrix2.");
                Console.WriteLine("\n Therefore Multiplication of Matrix1 with Matrix2 is not possible");
            }
        }

        #region "Inverse of a Matrix"
        /// <summary>
        /// Returns the inverse of a matrix with [n,n] dimension 
        /// and whose determinant is not zero.
        /// In case of an error the error is raised as an exception. 
        /// </summary>
        /// <param name="Mat">
        /// Array with [n,n] dimension whose inverse is to be found
        /// </param>
        /// <returns>Inverse of the array as an array</returns>
        public double[,] Inverse(double[,] Mat) {
            double[,] AI, Mat1;
            double AIN, AF;
            int Rows, Cols;
            int LL, LLM, L1, L2, LC, LCA, LCB;

            try {
                Find_R_C(Mat, out Rows, out Cols);
                Mat1 = (double[,])Mat.Clone();
            } catch { throw new MatrixNullException(); }

            if (Rows != Cols) throw new MatrixNotSquare();
            if (Det(Mat) == 0) throw new MatrixDeterminentZero();

            LL = Rows;
            LLM = Cols;
            AI = new double[LL + 1, LL + 1];

            for (L2 = 0; L2 <= LL; L2++) {
                for (L1 = 0; L1 <= LL; L1++) AI[L1, L2] = 0;
                AI[L2, L2] = 1;
            }

            for (LC = 0; LC <= LL; LC++) {
                if (Math.Abs(Mat1[LC, LC]) < 0.0000000001) {
                    for (LCA = LC + 1; LCA <= LL; LCA++) {
                        if (LCA == LC) continue;
                        if (Math.Abs(Mat1[LC, LCA]) > 0.0000000001) {
                            for (LCB = 0; LCB <= LL; LCB++) {
                                Mat1[LCB, LC] = Mat1[LCB, LC] + Mat1[LCB, LCA];
                                AI[LCB, LC] = AI[LCB, LC] + AI[LCB, LCA];
                            }
                            break;
                        }
                    }
                }
                AIN = 1 / Mat1[LC, LC];
                for (LCA = 0; LCA <= LL; LCA++) {
                    Mat1[LCA, LC] = AIN * Mat1[LCA, LC];
                    AI[LCA, LC] = AIN * AI[LCA, LC];
                }

                for (LCA = 0; LCA <= LL; LCA++) {
                    if (LCA == LC) continue;
                    AF = Mat1[LC, LCA];
                    for (LCB = 0; LCB <= LL; LCB++) {
                        Mat1[LCB, LCA] = Mat1[LCB, LCA] - AF * Mat1[LCB, LC];
                        AI[LCB, LCA] = AI[LCB, LCA] - AF * AI[LCB, LC];
                    }
                }
            }
            return AI;
        }

        #endregion

		private static void Find_R_C(double[,] Mat, out int Row, out int Col)
		{
			Row = Mat.GetUpperBound(0);
			Col = Mat.GetUpperBound(1);
		}

        #region "Determinant of a Matrix"
        /// <summary>
        /// Returns the determinant of a matrix with [n,n] dimension.
        /// In case of an error the error is raised as an exception. 
        /// </summary>
        /// <param name="Mat">
        /// Array with [n,n] dimension whose determinant is to be found
        /// </param>
        /// <returns>Determinant of the array</returns>
        public static double Det(double[,] Mat) {
            int S, k, k1, i, j;
            double[,] DArray;
            double save, ArrayK, tmpDet;
            int Rows, Cols;

            try {
                DArray = (double[,])Mat.Clone();
                Find_R_C(Mat, out Rows, out Cols);
            } catch { throw new MatrixNullException(); }

            if (Rows != Cols) throw new MatrixNotSquare();

            S = Rows;
            tmpDet = 1;

            for (k = 0; k <= S; k++) {
                if (DArray[k, k] == 0) {
                    j = k;
                    while ((j < S) && (DArray[k, j] == 0)) j = j + 1;
                    if (DArray[k, j] == 0) return 0;
                    else {
                        for (i = k; i <= S; i++) {
                            save = DArray[i, j];
                            DArray[i, j] = DArray[i, k];
                            DArray[i, k] = save;
                        }
                    }
                    tmpDet = -tmpDet;
                }
                ArrayK = DArray[k, k];
                tmpDet = tmpDet * ArrayK;
                if (k < S) {
                    k1 = k + 1;
                    for (i = k1; i <= S; i++) {
                        for (j = k1; j <= S; j++)
                            DArray[i, j] = DArray[i, j] - DArray[i, k] * (DArray[k, j] / ArrayK);
                    }
                }
            }
            return tmpDet;
        }
        #endregion

        #region "Exception in the Library"
        class MatrixLibraryExceptions : ApplicationException { public MatrixLibraryExceptions(string message) : base(message) { } }

        // The Exceptions in this Class
        class MatrixNullException : ApplicationException {
            public MatrixNullException() :
                base("To do this operation, matrix can not be null") { }
        }
        class MatrixDimensionException : ApplicationException {
            public MatrixDimensionException() :
                base("Dimension of the two matrices not suitable for this operation !") { }
        }
        class MatrixNotSquare : ApplicationException {
            public MatrixNotSquare() :
                base("To do this operation, matrix must be a square matrix !") { }
        }
        class MatrixDeterminentZero : ApplicationException {
            public MatrixDeterminentZero() :
                base("Determinent of matrix equals zero, inverse can't be found !") { }
        }
        class VectorDimensionException : ApplicationException {
            public VectorDimensionException() :
                base("Dimension of matrix must be [3 , 1] to do this operation !") { }
        }
        class MatrixSingularException : ApplicationException {
            public MatrixSingularException() :
                base("Matrix is singular this operation cannot continue !") { }
        }
        #endregion

    }
}
