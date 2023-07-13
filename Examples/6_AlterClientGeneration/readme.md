## This example shows how to customize the client generation process

You can customize client generation in 3 ways:

* Add parameter to client - example in `AlterClientGenerationTests.AddParameter`
* Transform existing parameter - example in `AlterClientGenerationTests.TransformParameter`
* Remove parameter - example in `AlterClientGenerationTests.RemoveParameter`

Test `AlterClientGenerationTests.TransformParameterWithManualMapping` shows how 
to use manual mapping in combination with customized client generation.

Full documentation about altering client generation can be found [here](https://github.com/Melchy/Ridge/wiki/6.-Customize-client-generation).