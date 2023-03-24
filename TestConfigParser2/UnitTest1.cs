using ConfigParser2;

namespace TestConfigParser2;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestSimpleReplacement()
    {
        var data = @"
        {
          ""key1"": ""${key2}"",
          ""key2"": ""value2""  
        }";
        var cr = new ConfigReader(data);
        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo("value2"));
        }
    }

    // Test that if a value contains two variables, both are replaced
    [Test]
    public void TestDoubleReplacement()
    {
        var data = @"
        {
          ""key1"": ""${key2}@${key3}"",
          ""key2"": ""value2"",
          ""key3"": ""value3""  
        }";
        var cr = new ConfigReader(data);
        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo(@"value2@value3"));
        }
    }

    // Test that if the replacement contains a variable, it is also replaced
    [Test]
    public void TestNestedReplacement()
    {
        var data = @"
        {
          ""key1"": ""${key2}"",
          ""key2"": ""value2@${key3}"",
          ""key3"": ""value3""  
        }";
        var cr = new ConfigReader(data);
        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo(@"value2@value3"));
        }
    }

    // Test that if a variable does not exist, an error it thrown
    [Test]
    public void TestMissingReplacement()
    {
        var data = @"
        {
          ""key1"": ""${key2}"",
          ""key2"": ""value2@${key3}"" 
        }";
        var cr = new ConfigReader(data);
        // Assert that Get throws an exception
        Assert.That(() => cr.Get("key1"), Throws.Exception);
    }

    // Test that an integer variable is correctly replaced
    [Test]
    public void TestIntegerReplacement()
    {
        var data = @"
        {
          ""key1"": ""${key2}"",
          ""key2"": 2 
        }";
        var cr = new ConfigReader(data);
        var r = cr.Get("key1");
        if (!(r is StringValue iv))
        {
            Assert.Fail("r is not an StringValue");
        }
        else
        {
            Assert.That(iv.Value, Is.EqualTo("2"));
        }
    }

    // Test that a true variable is correctly replaced
    [Test]
    public void TestTrueReplacement()
    {
        var data = @"
        {
          ""key1"": ""${key2}"",
          ""key2"": true 
        }";
        var cr = new ConfigReader(data);
        var r = cr.Get("key1");
        if (!(r is StringValue iv))
        {
            Assert.Fail("r is not an StringValue");
        }
        else
        {
            Assert.That(iv.Value, Is.EqualTo("true"));
        }
    }

    // Test that a false variable is correctly replaced
    [Test]
    public void TestFalseReplacement()
    {
        var data = @"
        {
          ""key1"": ""${key2}"",
          ""key2"": false 
        }";
        var cr = new ConfigReader(data);
        var r = cr.Get("key1");
        if (!(r is StringValue iv))
        {
            Assert.Fail("r is not an StringValue");
        }
        else
        {
            Assert.That(iv.Value, Is.EqualTo("false"));
        }
    }

    // Test that I can get a section and retrieve a value from it
    [Test]
    public void TestSection()
    {
        var data = @"
        {
          ""key1"": {
            ""key2"": ""value2""
          }
        }";
        var cr = new ConfigReader(data);
        var r = cr.GetSection("key1");
        if (r is null)
        {
            Assert.Fail("r is null");
        }
        else
        {
            var r2 = r.Get("key2");
            if (!(r2 is StringValue sv))
            {
                Assert.Fail("r2 is not a StringValue");
            }
            else
            {
                Assert.That(sv.Value, Is.EqualTo("value2"));
            }
        }
    }

    // Test that a null is returned if a section does not exist
    [Test]
    public void TestMissingSection()
    {
        var data = @"
        {
          ""key1"": {
            ""key2"": ""value2""
          }
        }";
        var cr = new ConfigReader(data);
        var r = cr.GetSection("key3");
        Assert.That(r, Is.Null);
    }

    // Test that a value from the chile section is returned if it does exist in the child section
    // and the parent section
    [Test]
    public void TestExistingValueInChildSection()
    {
        var data = @"
        {
          ""key1"": {
            ""key2"": ""value2""
          },
          ""key2"": ""value3""
        }";
        var cr = new ConfigReader(data);
        var r = cr.GetSection("key1");
        if (r is null)
        {
            Assert.Fail("r is null");
        }
        else
        {
            var r2 = r.Get("key2");
            if (!(r2 is StringValue sv))
            {
                Assert.Fail("r2 is not a StringValue");
            }
            else
            {
                Assert.That(sv.Value, Is.EqualTo("value2"));
            }
        }
    }

    // Test that the value from the parent section is returned if it does not exist in the child section
    [Test]
    public void TestMissingValueInChildSection()
    {
        var data = @"
        {
          ""key1"": {
            ""key2"": ""${key3}""
          },
          ""key3"": ""value3""
        }";
        var cr = new ConfigReader(data);
        var r = cr.GetSection("key1");
        if (r is null)
        {
            Assert.Fail("r is null");
        }
        else
        {
            var r2 = r.Get("key2");
            if (!(r2 is StringValue sv))
            {
                Assert.Fail("r2 is not a StringValue");
            }
            else
            {
                Assert.That(sv.Value, Is.EqualTo("value3"));
            }
        }
    }

    // Test that a value from the grandchild section is returned if it does exist in the grandchild section
    // and the parent section
    [Test]
    public void TestExistingValueInGrandChildSection()
    {
        var data = @"
        {
          ""key1"": {
            ""key2"": {
              ""key3"": ""${key4}""
            }
          },
          ""key4"": ""value4""
        }";
        var cr = new ConfigReader(data);
        var r = cr.GetSection("key1");
        if (r is null)
        {
            Assert.Fail("r is null");
        }
        else
        {
            var r2 = r.GetSection("key2");
            if (r2 is null)
            {
                Assert.Fail("r2 is null");
            }
            else
            {
                var r3 = r2.Get("key3");
                if (!(r3 is StringValue sv))
                {
                    Assert.Fail("r3 is not a StringValue");
                }
                else
                {
                    Assert.That(sv.Value, Is.EqualTo("value4"));
                }
            }
        }
    }

    // Test that a replacement from two diffenet sections is correctly replaced
    [Test]
    public void TestReplacementFromTwoSections()
    {
        var data = @"
        {
          ""key1"": {
            ""key2"": ""${key3}@${key4}"",
            ""key3"": ""value3""
          },
          ""key4"": ""value4""
        }";
        var cr = new ConfigReader(data);
        var r = cr.GetSection("key1");
        if (r is null)
        {
            Assert.Fail("r is null");
        }
        else
        {
            var r2 = r.Get("key2");
            if (!(r2 is StringValue sv))
            {
                Assert.Fail("r2 is not a StringValue");
            }
            else
            {
                Assert.That(sv.Value, Is.EqualTo("value3@value4"));
            }
        }
    }

    // Test that a path is correctly replaced
    [Test]
    public void TestPathReplacement()
    {
        var data = @"
        {
            ""key1"": ""${key3/}value"",
            ""key2"": ""${key4/}value"",
            ""key3"": ""value3"",
            ""key4"": ""value4/""
        }";
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo("value3" + Path.DirectorySeparatorChar + "value"));
        }
        r = cr.Get("key2");
        if (!(r is StringValue sv2))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            Assert.That(sv2.Value, Is.EqualTo("value4" + Path.DirectorySeparatorChar + "value"));
        }
    }
    
    // Test that environment variables are correctly replaced
    [Test]
    public void TestEnvironmentVariableReplacement()
    {
        var data = @"
        {
            ""key1"": ""${env:PATH}""
        }";
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo(Environment.GetEnvironmentVariable("PATH")));
        }
    }
    
    // Test that environment variables are correctly replaced, if the variable does not exist
    [Test]
    public void TestEnvironmentVariableReplacementWithDefault()
    {
        var name = Guid.NewGuid().ToString();
        var data = @"
        {
            ""key1"": ""${env:DUMMY:default}""
        }".Replace("DUMMY", name);
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo("default"));
        }
    }
    
    // Test that environment variables are correctly replaced, if the variable does exist
    [Test]
    public void TestEnvironmentVariableReplacementWithDefault2()
    {
        var name = Guid.NewGuid().ToString();
        var data = @"
        {
            ""key1"": ""${env:DUMMY:default}""
        }".Replace("DUMMY", name);
        Environment.SetEnvironmentVariable(name, "value");
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo("value"));
        }
    }
    
    // Test replacements in arrays
    [Test]
    public void TestReplacementInArray()
    {
        var data = @"
        {
            ""key1"": [
                ""${key2}"",
                ""${key3}""
            ],
            ""key2"": ""value2"",
            ""key3"": ""value3""
        }";
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is ArrayValue av))
        {
            Assert.Fail("r2 is not an ArrayValue");
        }
        else
        {
            Assert.That(av.Values.Count, Is.EqualTo(2));
            var value0 = (av.Values[0] as StringValue)?.Value;
            Assert.That(value0, Is.EqualTo("value2"));
            var value1 = (av.Values[1] as StringValue)?.Value;
            Assert.That(value1, Is.EqualTo("value3"));
        }
    }
    
    // Test decoding of base64 encoded values
    [Test]
    public void TestBase64Decoding()
    {
        var data = @"
        {
            ""key1"": ""${base64:SGVsbG8sIFdvcmxkIQ==}""
        }";
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            Assert.That(sv.Value, Is.EqualTo("Hello, World!"));
        }
    }
    
    // Test file reading replacement
    [Test]
    public void TestFileReading()
    {
        var tmpName = Path.GetTempFileName();
        File.WriteAllText(tmpName, "Hello, World!");
        try
        {
            var data = @"
            {
                ""key1"": ""${file:DUMMY}""
            }".Replace("DUMMY", tmpName.Replace("\\", "\\\\")); // Escape backslashes
            var cr = new ConfigReader(data);

            var r = cr.Get("key1");
            if (!(r is StringValue sv))
            {
                Assert.Fail("r2 is not a StringValue");
            }
            else
            {
                Assert.That(sv.Value, Is.EqualTo("Hello, World!"));
            }
        }
        finally
        {
            File.Delete(tmpName);
        }
    }
    
    // Test now replacement
    [Test]
    public void TestNowReplacement()
    {
        var data = @"
        {
            ""key1"": ""${now}""
        }";
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            var value = DateTime.Parse(sv.Value);
            Assert.That(value, Is.EqualTo(DateTime.Now).Within(5).Seconds);
        }
    }
    
    // Test utcnow replacement
    [Test]
    public void TestUtcNowReplacement()
    {
        var data = @"
        {
            ""key1"": ""${utcnow}""
        }";
        var cr = new ConfigReader(data);

        var r = cr.Get("key1");
        if (!(r is StringValue sv))
        {
            Assert.Fail("r2 is not a StringValue");
        }
        else
        {
            var value = DateTime.Parse(sv.Value);
            Assert.That(value, Is.EqualTo(DateTime.UtcNow).Within(5).Seconds);
        }
    }
}