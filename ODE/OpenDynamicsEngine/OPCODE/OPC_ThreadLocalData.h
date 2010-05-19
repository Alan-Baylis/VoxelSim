///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
*	OPCODE - Optimized Collision Detection
*	Copyright (C) 2001 Pierre Terdiman
*	Homepage: http://www.codercorner.com/Opcode.htm
*/
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**
*	Contains declaration of thread local data structure.
*	\file		OPC_ThreadLocalData.h
*	\author		Oleh Derevenko
*	\date		April, 16, 2008
*/
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Include Guard
#ifndef __OPC_THREADLOCALDATA_H__
#define __OPC_THREADLOCALDATA_H__


// InsertionSort has better coherence, RadixSort is better for one-shot queries.
typedef RadixSort PRUNING_SORTER;
//typedef InsertionSort PRUNING_SORTER

struct ThreadLocalData
{
public:
	ThreadLocalData() { Init(); }
	~ThreadLocalData() { Finit(); }

protected:
	ThreadLocalData(bool): gCompletePruningSorter(0), 
		gBipartitePruningSorter0(0), gBipartitePruningSorter1(0)
	{
	}

	void Init();
	void Finit();

public:
	// Static for coherence
	PRUNING_SORTER* gCompletePruningSorter;
	PRUNING_SORTER* gBipartitePruningSorter0;
	PRUNING_SORTER* gBipartitePruningSorter1;
};

typedef ThreadLocalData *(* ThreadLocalDataProviderProc)();


#endif // __OPC_THREADLOCALDATA_H__

