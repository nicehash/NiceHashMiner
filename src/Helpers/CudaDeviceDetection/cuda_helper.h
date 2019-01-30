#pragma once

#include <cuda.h>
#include <cuda_runtime.h>

#define CUDA_SAFE_CALL(call)								\
do {														\
	cudaError_t err = call;									\
	if (cudaSuccess != err) {								\
		const char * errorString = cudaGetErrorString(err);	\
		fprintf(stderr,										\
			"CUDA error in func '%s' at line %i : %s.\n",	\
			__FUNCTION__, __LINE__, errorString);			\
		throw std::runtime_error(errorString);				\
				}											\
} while (0)

