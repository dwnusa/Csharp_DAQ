
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <math.h>
//#include "../common/book.h"
//#include "../common/cpu_bitmap.h"

#define NN (640 * 480)//(33*1024)
#define length (640 * 480)
#define MIN(X,Y) ((X) < (Y) ? (X) : (Y))  
#define MAX(X,Y) ((X) > (Y) ? (X) : (Y))  
cudaError_t normconvWithCuda(double *b, const double *a, const int width, const int height, const int fwidth, const double min, const double max);
cudaError_t convWithCuda(double *b, const double *a, const int width, const int height, const int fwidth);
cudaError_t smoothingWithCuda(double *b, const double *a, const double filter_size, const int size);
cudaError_t normWithCuda(double *b, const double *a, const double min, const double max, const int size);
cudaError_t addWithCuda(int *c, const int *a, const int *b, unsigned int size);

__global__ void MatrixNormConv(double*P, double*M, double*N, int Width, int Height, int fWidth, double fsum, double min, double max)
{
	int tid, tx, ty;
	//2차원 작업 분할 인덱스 계산
	tx = blockDim.x * blockIdx.x + threadIdx.x;
	ty = blockDim.y * blockIdx.y + threadIdx.y;
	tid = Width * ty + tx;
	//filter 변수 
	int fPad;
	fPad = fWidth / 2;
	int index_x = 0;
	int index_y = 0;

	double Value = 0;
	double MVal = 0;
	double NVal = 0;

	P[tid] = (M[tid] - min) / (max - min) * 255;

	//if (tx >= fPad && tx < Width - fPad) {
	//	if (ty >= fPad && ty < Height - fPad) {
	//		for (index_y = -fPad; index_y <= fPad; index_y++) { //필터크기만큼 순환
	//			for (index_x = -fPad; index_x <= fPad; index_x++)
	//			{
	//				int fcol = ty + index_y;
	//				int frow = tx + index_x;
	//				int findex = fcol*Width + frow;
	//				int findex_y = index_y + fPad;
	//				int findex_x = index_x + fPad;
	//				int index = findex_y * fWidth + findex_x;
	//				MVal = M[findex];
	//				NVal = N[index];
	//				Value += MVal * NVal;
	//			}
	//		}
	//		P[tid] = Value / 9.0;
	//	}
	//}
}
__global__ void smoothingKernel(double *b, const double *a, double *f, const double filter_size, const double filter_sum, const int size)
{
	double value = 0;
	int filter_pad = filter_size / 2;
	int halfy = filter_pad;
	int halfx = filter_pad;
	int tid = threadIdx.x + blockIdx.x * blockDim.x;
	int x = tid % 640;
	int y = tid / 640;
	//while (tid >= filter_pad && tid < size-filter_pad) {
	//	double value = 0;
	//for (int n = -halfy; n <= halfy; n++) 
	//{
	//	for (int m = -halfx; m <= halfx; m++)
	//	{
	//		value += (double)a[   ( ( ( y+n ) * 640) + x + m ) ] *      f[ (n + halfy) * (int)(filter_size)    + (m + halfx) ];
	//		//value += (double)a[ ( ( ( y+n ) * 20 ) + x + m ) ] * matrix[ (n + halfy) * (int)filter_size + (m + halfx) ];
	//	}
	//}
	//value = value / filter_sum;
	//b[tid] = value;
	b[tid] = a[tid];
	//	tid += blockDim.x * gridDim.x;
	//}
}

__global__ void normKernel(double *b, const double *a, const double min, const double max, const int size)
{
	int tid = threadIdx.x + blockIdx.x * blockDim.x;
	while (tid < size) {
		b[tid] = (a[tid] - min) / (max - min) * 255;
		tid += blockDim.x * gridDim.x;
	}
}


__global__ void addKernel(int *c, const int *a, const int *b)
{
	int i = threadIdx.x;
	c[i] = a[i] + b[i];
}
extern "C" __declspec(dllexport) void exportCppFunctionNormConv(double* dst, double* src, int width, int height, int fwidth, double min, double max)
{
	//	printf("exportCppFunctionNorm \n");

	cudaError_t cudaStatus = normconvWithCuda(dst, src, width, height, fwidth, min, max);
	if (cudaStatus != cudaSuccess) fprintf(stderr, "normconvWithCuda failed!");
	//else printf("exportCppFunctionNorm Success \n");
}
extern "C" __declspec(dllexport) void exportCppFunctionConv(double* dst, double* src, int width, int height, int fwidth)
{
	//	printf("exportCppFunctionNorm \n");

	cudaError_t cudaStatus = convWithCuda(dst, src, width, height, fwidth);
	if (cudaStatus != cudaSuccess) fprintf(stderr, "convWithCuda failed!");
	//else printf("exportCppFunctionNorm Success \n");
}

extern "C" __declspec(dllexport) void exportCppFunctionSmoothing(double* dst, double* src, double filter_size, int arraySize)
{
	//	printf("exportCppFunctionNorm \n");

	cudaError_t cudaStatus = smoothingWithCuda(dst, src, filter_size, arraySize);
	if (cudaStatus != cudaSuccess) fprintf(stderr, "smoothingWithCuda failed!");
	//else printf("exportCppFunctionNorm Success \n");
}


extern "C" __declspec(dllexport) void exportCppFunctionNorm(double* dst, double* src, double min, double max, int arraySize)
{
	//	printf("exportCppFunctionNorm \n");

	cudaError_t cudaStatus = normWithCuda(dst, src, min, max, arraySize);
	if (cudaStatus != cudaSuccess) fprintf(stderr, "normWithCuda failed!");
	//else printf("exportCppFunctionNorm Success \n");
}


extern "C" __declspec(dllexport) void exportCppFunctionAdd(int* src, int* src2, int* dst, int arraySize)
{
	printf("exportCppFunctionAdd \n");

	cudaError_t cudaStatus = addWithCuda(dst, src, src2, arraySize);
	if (cudaStatus != cudaSuccess) fprintf(stderr, "addWithCuda failed!");
	else printf("exportCppFunctionAdd Success \n");
}
__global__ void VectorAdd(const double*a, const double*b, double*c, double size)
{
	int tid = blockIdx.x * blockDim.x + threadIdx.x;
	c[tid] = a[tid] + b[tid];
}
__global__ void add(double *a, double *b, double *c)
{
	int tid = threadIdx.x + blockIdx.x * blockDim.x;
	//while (tid < N) {
	c[tid] = a[tid] + b[tid];
	//	tid += blockDim.x * gridDim.x;
	//}
}
__global__ void MatrixConv(double*P, double*M, double*N, int Width, int Height, int fWidth, double fsum)
{
	int tid, tx, ty;
	//2차원 작업 분할 인덱스 계산
	tx = blockDim.x * blockIdx.x + threadIdx.x;
	ty = blockDim.y * blockIdx.y + threadIdx.y;
	tid = Width * ty + tx;
	//filter 변수 
	int fPad;
	fPad = fWidth / 2;
	int index_x = 0;
	int index_y = 0;

	double Value = 0;
	double MVal = 0;
	double NVal = 0;

	if (tx >= fPad && tx < Width - fPad) {
		if (ty >= fPad && ty < Height - fPad) {
			for (index_y = -fPad; index_y <= fPad; index_y++) { //필터크기만큼 순환
				for (index_x = -fPad; index_x <= fPad; index_x++)
				{
					int fcol = ty + index_y;
					int frow = tx + index_x;
					int findex = fcol*Width + frow;
					int findex_y = index_y + fPad;
					int findex_x = index_x + fPad;
					int index = findex_y * fWidth + findex_x;
					MVal = M[findex];
					NVal = N[index];
					Value += MVal * NVal;
				}
			}
			P[tid] = Value / 9.0;
		}
	}
}
void MatrixConvC(int*M, int*N, int*P, int Width, int Height, int fWidth) {
	int col = 0;
	int raw = 0;
	int index_x = 0;
	int index_y = 0;
	int Destindex = 0;

	//filter 변수 
	int fPad;
	fPad = fWidth / 2;

	for (col = fPad; col < Height - fPad; col++) {
		for (raw = fPad; raw < Width - fPad; raw++) {
			Destindex = col*Width + raw;
			for (index_y = -fPad; index_y <= fPad; index_y++) {
				for (index_x = -fPad; index_x <= fPad; index_x++) {
					int fcol = col + index_y;
					int frow = raw + index_x;
					int findex = fcol*Width + frow;
					int findex_y = index_y + fPad;
					int findex_x = index_x + fPad;
					int index = findex_y * fWidth + findex_x;
					P[Destindex] += M[findex] * N[index];
				}
			}
			P[Destindex] = P[Destindex] / 9;
		}
	}
}
int main()
{
	//const int MatrixWidth = 640;
	//const int MatrixHeight = 480;
	//const int MatrixSize = MatrixWidth*MatrixHeight;
	//const int BufferSize1 = MatrixSize * sizeof(int);

	//const int FilterWidth = 5;
	//const int FilterHeight = 5;
	//const int FilterSize = FilterWidth*FilterHeight;
	//const int BufferSize2 = FilterSize * sizeof(int);

	//int* M;
	//int* N;
	//int* P_cuda;
	//int* P_C;

	////호스트 메모리 할당
	//M = (int*)malloc(BufferSize1);
	//N = (int*)malloc(BufferSize2);
	//P_cuda = (int*)malloc(BufferSize1);
	//P_C = (int*)malloc(BufferSize1);

	//int i, j = 0;

	////데이터 입력
	//for (int i = 0; i < MatrixSize; i++) {
	//	M[i] = i;
	//	//N[i] = i;
	//	P_cuda[i] = 0;
	//	P_C[i] = 0;
	//}
	//for (int i = 0; i < FilterSize; i++) {
	//	N[i] = 1;
	//}

	//int* dev_M;
	//int* dev_N;
	//int* dev_P;

	////디바이스 메모리 할당
	//cudaMalloc((void**)&dev_M, BufferSize1);
	//cudaMalloc((void**)&dev_N, BufferSize2);
	//cudaMalloc((void**)&dev_P, BufferSize1);

	////호스트 디바이스 입력 데이터 전송
	//cudaMemcpy(dev_M, M, BufferSize1, cudaMemcpyHostToDevice);
	//cudaMemcpy(dev_N, N, BufferSize2, cudaMemcpyHostToDevice);

	////dim3 Dg(3, 4, 1);
	//dim3 Dg(20, 32, 1);
	////dim3 Db(20, 32, 1);
	////dim3 Db(4, 3, 1);
	//dim3 Db(32, 15, 1);

	////CUDA kernel 매트릭스 곱 계산
	//MatrixConv << <Dg, Db >> > (dev_P, dev_M, dev_N,  MatrixWidth, MatrixHeight, FilterWidth);

	////디바이스 호스트 출력 데이터 전송
	//cudaMemcpy(P_cuda, dev_P, BufferSize1, cudaMemcpyDeviceToHost);

	////C 함수 매트릭스 곱 계산
	//MatrixConvC(M, N, P_C, MatrixWidth, MatrixHeight, FilterWidth);

	//bool ResultFlag = true;
	////결과 출력
	//for (i = 0; i < MatrixSize; i++) {
	//	//printf("Result[%d] : %d, %d\n",i,P_cuda[i], P_C[i]);
	//	if (P_cuda[i] != P_C[i]) ResultFlag = false;
	//}
	//for (i = 0; i < 12; i++) {
	//	for (j = 0; j < 12; j++) {
	//		printf("Result[%d, %d] : %d, %d\n", i, j, P_cuda[i*MatrixHeight + j], P_C[i*MatrixHeight + j]);
	//		//if (P_cuda[i] != P_C[i]) ResultFlag = false;
	//	}
	//}
	////MatrixWidth MatrixHeight

	//if (ResultFlag == true) printf("MatrixMul Result OK!\n");
	//else printf("MatrixMul Result Error!\n");

	//cudaFree(dev_M);
	//cudaFree(dev_N);
	//cudaFree(dev_P);

	//free(M);
	//free(N);
	//free(P_cuda);
	//free(P_C);

	return 0;
}
cudaError_t normconvWithCuda(double *b, const double *a, const int Width, const int Height, const int fWidth, double min, double max)
{
	double *dev_a = 0;
	double *dev_b = 0;
	double *dev_c = 0;
	double *dev_f = 0;
	cudaError_t cudaStatus;
	/////////////////////////////////////////////////////////////////////////////////
	//image size
	int size = Width * Height;
	//Prepare to filter
	double f_size = fWidth;
	double total_size = f_size * f_size;
	double sigma = 4.0;
	double* matrix = new double[(int)total_size];
	//double* matrix = (double*)malloc((int)f_size * sizeof(double));
	//for (int x = 0; x < f_size; x++)
	//	matrix[x] = new double[f_size];
	int halfx = (int)f_size / 2;
	int halfy = (int)f_size / 2;
	double denom = -2.0 * pow(sigma, 2);
	double sum = 0.0;
	for (int y = -halfy; y <= halfy; y++) {
		int i = y + halfy;
		for (int x = -halfx; x <= halfx; x++) {
			int j = x + halfx;
			//Gaussian 함수 값 계산하기
			double value = (double)exp((double)(x*x + y*y) / denom); //1/pow(filter_size,2);//
			matrix[(i*(int)f_size) + j] = value;
			sum += value;
		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Choose which GPU to run on, change this on a multi-GPU system.
	cudaStatus = cudaSetDevice(0);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaSetDevice failed!  Do you have a CUDA-capable GPU installed?");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_f, total_size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}
	cudaStatus = cudaMalloc((void**)&dev_c, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}
	cudaStatus = cudaMalloc((void**)&dev_b, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_a, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	// Copy input vectors from host memory to GPU buffers.
	cudaStatus = cudaMemcpy(dev_f, matrix, total_size * sizeof(double), cudaMemcpyHostToDevice);////////////////////////////////////
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	cudaStatus = cudaMemcpy(dev_a, a, size * sizeof(double), cudaMemcpyHostToDevice);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	// Launch a kernel on the GPU with one thread for each element.
	//smoothingKernel << <640, 480 >> >(dev_b, dev_a, dev_f, f_size, sum, size);

	dim3 Dg(20, 32, 1);
	dim3 Db(32, 15, 1);
	//CUDA kernel 매트릭스 곱 계산
	MatrixNormConv << <Dg, Db >> > (dev_b, dev_a, dev_f, Width, Height, fWidth, sum, min, max);

	MatrixConv << <Dg, Db >> > (dev_c, dev_b, dev_f, Width, Height, fWidth, sum);
	//smoothingKernel1(dev_b, dev_a, dev_f, halfx, sum, size);
	// Check for any errors launching the kernel
	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "smoothingKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
		goto Error;
	}

	// cudaDeviceSynchronize waits for the kernel to finish, and returns
	// any errors encountered during the launch.
	cudaStatus = cudaDeviceSynchronize();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching smoothingKernel!\n", cudaStatus);
		goto Error;
	}

	// Copy output vector from GPU buffer to host memory.
	cudaStatus = cudaMemcpy(b, dev_c, size * sizeof(double), cudaMemcpyDeviceToHost);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

Error:
	cudaFree(dev_a);
	cudaFree(dev_b);
	cudaFree(dev_c);
	cudaFree(dev_f);
	//free(matrix);
	delete[] matrix;
	return cudaStatus;
}
cudaError_t convWithCuda(double *b, const double *a, const int Width, const int Height, const int fWidth)
{
	double *dev_a = 0;
	double *dev_b = 0;
	double *dev_f = 0;
	cudaError_t cudaStatus;
	/////////////////////////////////////////////////////////////////////////////////
	//image size
	int size = Width * Height;
	//Prepare to filter
	double f_size = fWidth;
	double total_size = f_size * f_size;
	double sigma = 4.0;
	double* matrix = new double[(int)total_size];
	//double* matrix = (double*)malloc((int)f_size * sizeof(double));
	//for (int x = 0; x < f_size; x++)
	//	matrix[x] = new double[f_size];
	int halfx = (int)f_size / 2;
	int halfy = (int)f_size / 2;
	double denom = -2.0 * pow(sigma, 2);
	double sum = 0.0;
	for (int y = -halfy; y <= halfy; y++) {
		int i = y + halfy;
		for (int x = -halfx; x <= halfx; x++) {
			int j = x + halfx;
			//Gaussian 함수 값 계산하기
			double value = (double)exp((double)(x*x + y*y) / denom); //1/pow(filter_size,2);//
			matrix[(i*(int)f_size) + j] = value;
			sum += value;
		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Choose which GPU to run on, change this on a multi-GPU system.
	cudaStatus = cudaSetDevice(0);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaSetDevice failed!  Do you have a CUDA-capable GPU installed?");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_f, total_size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_b, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_a, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	// Copy input vectors from host memory to GPU buffers.
	cudaStatus = cudaMemcpy(dev_f, matrix, total_size * sizeof(double), cudaMemcpyHostToDevice);////////////////////////////////////
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	cudaStatus = cudaMemcpy(dev_a, a, size * sizeof(double), cudaMemcpyHostToDevice);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	// Launch a kernel on the GPU with one thread for each element.
	//smoothingKernel << <640, 480 >> >(dev_b, dev_a, dev_f, f_size, sum, size);

	dim3 Dg(20, 32, 1);
	dim3 Db(32, 15, 1);
	//CUDA kernel 매트릭스 곱 계산
	MatrixConv << <Dg, Db >> > (dev_b, dev_a, dev_f, Width, Height, fWidth, sum);
	//smoothingKernel1(dev_b, dev_a, dev_f, halfx, sum, size);
	// Check for any errors launching the kernel
	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "smoothingKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
		goto Error;
	}

	// cudaDeviceSynchronize waits for the kernel to finish, and returns
	// any errors encountered during the launch.
	cudaStatus = cudaDeviceSynchronize();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching smoothingKernel!\n", cudaStatus);
		goto Error;
	}

	// Copy output vector from GPU buffer to host memory.
	cudaStatus = cudaMemcpy(b, dev_b, size * sizeof(double), cudaMemcpyDeviceToHost);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

Error:
	cudaFree(dev_a);
	cudaFree(dev_b);
	cudaFree(dev_f);
	//free(matrix);
	delete[] matrix;
	return cudaStatus;
}
cudaError_t smoothingWithCuda(double *b, const double *a, const double filter_size, const int size)
{
	double *dev_a = 0;
	double *dev_b = 0;
	double *dev_f = 0;
	cudaError_t cudaStatus;
	/////////////////////////////////////////////////////////////////////////////////
	//Prepare to filter
	double f_size = filter_size;
	double total_size = f_size * f_size;
	double sigma = 4.0;
	double* matrix = new double[(int)total_size];
	//double* matrix = (double*)malloc((int)f_size * sizeof(double));
	//for (int x = 0; x < f_size; x++)
	//	matrix[x] = new double[f_size];
	int halfx = (int)f_size / 2;
	int halfy = (int)f_size / 2;
	double denom = -2.0 * pow(sigma, 2);
	double sum = 0.0;
	for (int y = -halfy; y <= halfy; y++) {
		int i = y + halfy;
		for (int x = -halfx; x <= halfx; x++) {
			int j = x + halfx;
			//Gaussian 함수 값 계산하기
			double value = (double)exp((double)(x*x + y*y) / denom); //1/pow(filter_size,2);//
			matrix[(i*(int)f_size) + j] = value;
			sum += value;
		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Choose which GPU to run on, change this on a multi-GPU system.
	cudaStatus = cudaSetDevice(0);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaSetDevice failed!  Do you have a CUDA-capable GPU installed?");
		goto Error;
	}

	// Allocate GPU buffers for three vectors (two input, one output).
	cudaStatus = cudaMalloc((void**)&dev_f, total_size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_b, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_a, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	// Copy input vectors from host memory to GPU buffers.
	cudaStatus = cudaMemcpy(dev_f, matrix, total_size * sizeof(double), cudaMemcpyHostToDevice);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	cudaStatus = cudaMemcpy(dev_a, a, size * sizeof(double), cudaMemcpyHostToDevice);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	// Launch a kernel on the GPU with one thread for each element.
	smoothingKernel << <640, 480 >> >(dev_b, dev_a, dev_f, f_size, sum, size);
	//smoothingKernel1(dev_b, dev_a, dev_f, halfx, sum, size);
	// Check for any errors launching the kernel
	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "smoothingKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
		goto Error;
	}

	// cudaDeviceSynchronize waits for the kernel to finish, and returns
	// any errors encountered during the launch.
	cudaStatus = cudaDeviceSynchronize();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching smoothingKernel!\n", cudaStatus);
		goto Error;
	}

	// Copy output vector from GPU buffer to host memory.
	cudaStatus = cudaMemcpy(b, dev_b, size * sizeof(double), cudaMemcpyDeviceToHost);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

Error:
	cudaFree(dev_a);
	cudaFree(dev_b);
	cudaFree(dev_f);
	//free(matrix);
	delete[] matrix;
	return cudaStatus;
}



cudaError_t normWithCuda(double *b, const double *a, const double min, const double max, const int size)
{
	double *dev_a = 0;
	double *dev_b = 0;
	cudaError_t cudaStatus;

	// Choose which GPU to run on, change this on a multi-GPU system.
	cudaStatus = cudaSetDevice(0);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaSetDevice failed!  Do you have a CUDA-capable GPU installed?");
		goto Error;
	}

	// Allocate GPU buffers for three vectors (two input, one output).
	cudaStatus = cudaMalloc((void**)&dev_b, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_a, size * sizeof(double));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	// Copy input vectors from host memory to GPU buffers.
	cudaStatus = cudaMemcpy(dev_a, a, size * sizeof(double), cudaMemcpyHostToDevice);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	// Launch a kernel on the GPU with one thread for each element.
	normKernel << <640, 480 >> >(dev_b, dev_a, min, max, size);

	// Check for any errors launching the kernel
	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "normKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
		goto Error;
	}

	// cudaDeviceSynchronize waits for the kernel to finish, and returns
	// any errors encountered during the launch.
	cudaStatus = cudaDeviceSynchronize();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching normKernel!\n", cudaStatus);
		goto Error;
	}

	// Copy output vector from GPU buffer to host memory.
	cudaStatus = cudaMemcpy(b, dev_b, size * sizeof(double), cudaMemcpyDeviceToHost);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

Error:
	cudaFree(dev_a);
	cudaFree(dev_b);

	return cudaStatus;
}
// Helper function for using CUDA to add vectors in parallel.
cudaError_t addWithCuda(int *c, const int *a, const int *b, unsigned int size)
{
	int *dev_a = 0;
	int *dev_b = 0;
	int *dev_c = 0;
	cudaError_t cudaStatus;

	// Choose which GPU to run on, change this on a multi-GPU system.
	cudaStatus = cudaSetDevice(0);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaSetDevice failed!  Do you have a CUDA-capable GPU installed?");
		goto Error;
	}

	// Allocate GPU buffers for three vectors (two input, one output)    .
	cudaStatus = cudaMalloc((void**)&dev_c, size * sizeof(int));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_a, size * sizeof(int));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	cudaStatus = cudaMalloc((void**)&dev_b, size * sizeof(int));
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMalloc failed!");
		goto Error;
	}

	// Copy input vectors from host memory to GPU buffers.
	cudaStatus = cudaMemcpy(dev_a, a, size * sizeof(int), cudaMemcpyHostToDevice);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	cudaStatus = cudaMemcpy(dev_b, b, size * sizeof(int), cudaMemcpyHostToDevice);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

	// Launch a kernel on the GPU with one thread for each element.
	addKernel << <1, size >> >(dev_c, dev_a, dev_b);

	// Check for any errors launching the kernel
	cudaStatus = cudaGetLastError();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "addKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
		goto Error;
	}

	// cudaDeviceSynchronize waits for the kernel to finish, and returns
	// any errors encountered during the launch.
	cudaStatus = cudaDeviceSynchronize();
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching addKernel!\n", cudaStatus);
		goto Error;
	}

	// Copy output vector from GPU buffer to host memory.
	cudaStatus = cudaMemcpy(c, dev_c, size * sizeof(int), cudaMemcpyDeviceToHost);
	if (cudaStatus != cudaSuccess) {
		fprintf(stderr, "cudaMemcpy failed!");
		goto Error;
	}

Error:
	cudaFree(dev_c);
	cudaFree(dev_a);
	cudaFree(dev_b);

	return cudaStatus;
}
