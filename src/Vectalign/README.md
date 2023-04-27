## C# Port of https://github.com/bonnyfone/vectalign

Copyright 2015, Stefano Bonetta.
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

### CHANGES:
i made interfaces from the abstract fillmode and alignment classes
i removed android specific code and rolled my own svg path parser
i used a Commandtype enum instead of using chars (might have been stupid)
i removed the console chatter, and used debuggerdisplay instead (where applicable)