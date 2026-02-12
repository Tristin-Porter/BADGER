;; Test WAT Module for Complete x86_64 MapSet
;; This module tests various features of the complete lowering implementation

(module
  ;; Test 1: Simple arithmetic
  (func $add (param $a i32) (param $b i32) (result i32)
    local.get $a
    local.get $b
    i32.add
  )

  ;; Test 2: Multiple operations
  (func $complex (param $x i32) (param $y i32) (result i32)
    local.get $x
    i32.const 10
    i32.mul
    local.get $y
    i32.const 5
    i32.div_s
    i32.add
  )

  ;; Test 3: Comparisons and control flow
  (func $max (param $a i32) (param $b i32) (result i32)
    local.get $a
    local.get $b
    i32.gt_s
    if (result i32)
      local.get $a
    else
      local.get $b
    end
  )

  ;; Test 4: Loops
  (func $factorial (param $n i32) (result i32)
    (local $result i32)
    (local $i i32)
    
    i32.const 1
    local.set $result
    
    i32.const 1
    local.set $i
    
    block $break
      loop $continue
        local.get $i
        local.get $n
        i32.gt_s
        br_if $break
        
        local.get $result
        local.get $i
        i32.mul
        local.set $result
        
        local.get $i
        i32.const 1
        i32.add
        local.set $i
        
        br $continue
      end
    end
    
    local.get $result
  )

  ;; Test 5: Local variables
  (func $locals_test (param $p i32) (result i32)
    (local $a i32)
    (local $b i32)
    (local $c i32)
    
    local.get $p
    i32.const 5
    i32.add
    local.tee $a
    local.tee $b
    i32.const 3
    i32.mul
    local.set $c
    
    local.get $a
    local.get $b
    i32.add
    local.get $c
    i32.add
  )

  ;; Test 6: Bitwise operations
  (func $bitwise (param $x i32) (result i32)
    local.get $x
    i32.const 0xFF
    i32.and
    i32.const 8
    i32.shl
    local.get $x
    i32.const 0xFF00
    i32.and
    i32.const 8
    i32.shr_u
    i32.or
  )

  ;; Test 7: Memory operations (requires memory section)
  (memory 1)
  
  (func $mem_test (param $addr i32) (param $value i32) (result i32)
    ;; Store value at address
    local.get $addr
    local.get $value
    i32.store
    
    ;; Load and return
    local.get $addr
    i32.load
  )

  ;; Test 8: All comparison types
  (func $comparisons (param $a i32) (param $b i32) (result i32)
    local.get $a
    local.get $b
    i32.eq
    if (result i32)
      i32.const 1
    else
      local.get $a
      local.get $b
      i32.ne
      if (result i32)
        i32.const 2
      else
        local.get $a
        local.get $b
        i32.lt_s
        if (result i32)
          i32.const 3
        else
          i32.const 4
        end
      end
    end
  )

  ;; Test 9: Select instruction
  (func $select_test (param $a i32) (param $b i32) (param $cond i32) (result i32)
    local.get $a
    local.get $b
    local.get $cond
    select
  )

  ;; Test 10: Multiple return points
  (func $early_return (param $x i32) (result i32)
    local.get $x
    i32.const 0
    i32.eq
    if (result i32)
      i32.const 0
      return
    end
    
    local.get $x
    i32.const 1
    i32.eq
    if (result i32)
      i32.const 1
      return
    end
    
    local.get $x
    local.get $x
    i32.mul
  )

  ;; Test 11: Nested blocks
  (func $nested_blocks (param $x i32) (result i32)
    block $outer
      block $inner
        local.get $x
        i32.const 0
        i32.eq
        br_if $outer
        
        local.get $x
        i32.const 1
        i32.eq
        br_if $inner
        
        i32.const 100
        return
      end
      i32.const 50
      return
    end
    i32.const 0
  )

  ;; Test 12: i64 operations
  (func $i64_ops (param $a i64) (param $b i64) (result i64)
    local.get $a
    local.get $b
    i64.add
    i64.const 2
    i64.div_s
  )

  ;; Test 13: Type conversions
  (func $conversions (param $x i32) (result i64)
    local.get $x
    i64.extend_i32_s
    i64.const 0xFF
    i64.and
  )

  ;; Test 14: Remainder operations
  (func $modulo (param $a i32) (param $b i32) (result i32)
    local.get $a
    local.get $b
    i32.rem_s
  )

  ;; Test 15: Shift and rotate
  (func $shifts (param $value i32) (param $amount i32) (result i32)
    local.get $value
    local.get $amount
    i32.shl
    local.get $value
    local.get $amount
    i32.rotl
    i32.xor
  )

  ;; Export some functions for testing
  (export "add" (func $add))
  (export "max" (func $max))
  (export "factorial" (func $factorial))
  (export "mem_test" (func $mem_test))
)
