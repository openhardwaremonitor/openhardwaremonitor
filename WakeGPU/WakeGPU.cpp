/*

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, You can obtain one at http://mozilla.org/MPL/2.0/.

Copyright (C) 2017 Rick Yorgason <rick@firefang.com>

*/

// This project simply spins the GPU for 10 seconds, or until it's killed.
// Its purpose is to wake any dormant GPUs on laptops so all their information
// is available when OpenHardwareMonitor launches.

#include <Windows.h>
#include <d3d9.h>
#include <stdio.h>
#include <time.h>

// These exports tell the system to use the dedicated GPU instead of the the
// integrated one.
// This is the reason why this C++ project is necessary; C# doesn't have any
// particularily reasonable way to export variables.
extern "C"
{
	__declspec(dllexport) DWORD NvOptimusEnablement = 1;
	__declspec(dllexport) int AmdPowerXpressRequestHighPerformance = 1;
}

int main()
{
	auto timeStart = time(nullptr);

	// Ideally our parent process will shut us down before the 10 seconds is up,
	// but just to be sure, we'll shut ourselves down after 10 seconds.
	while (time(nullptr) < timeStart + 10)
	{
		IDirect3D9* d3dobject = Direct3DCreate9(D3D_SDK_VERSION);

		// We use this in the parent program to detect when the program is running.
		printf("Spinning\n");

		if(!FAILED(d3dobject))
			d3dobject->Release();
	}
}