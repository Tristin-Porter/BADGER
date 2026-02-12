(module
  (func $add (param i32 i32) (result i32)
    local.get 0
    local.get 1
    i32.add
  )
  
  (func $main (result i32)
    i32.const 10
    i32.const 32
    call $add
  )
)
